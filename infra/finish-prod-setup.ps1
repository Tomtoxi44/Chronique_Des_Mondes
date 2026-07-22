# Deux dernieres etapes de cablage de la prod (le reste est deja fait) :
#  1. l'API (identite managee systeme) doit pouvoir ecrire dans le conteneur images
#  2. l'envoi de mail doit passer par Azure Communication Services au lieu du logger
#
# A lancer une seule fois. Les app settings persistent entre les deploiements.

$rg  = 'rg-chronique-des-mondes-app'
$api = 'app-chroniquedesmondes-api'
$sa  = 'stcdmchroniquemondes'

# --- 1. Role blob pour l'identite managee de l'API ---------------------------
$apiPrincipalId = az webapp identity show -g $rg -n $api --query principalId -o tsv
$saId           = az storage account show -g $rg -n $sa --query id -o tsv

az role assignment create `
    --assignee-object-id $apiPrincipalId `
    --assignee-principal-type ServicePrincipal `
    --role 'Storage Blob Data Contributor' `
    --scope $saId

# --- 2. Envoi de mail via ACS ------------------------------------------------
# La chaine contient une cle : elle n'est jamais affichee, juste transmise.
$acsConnectionString = az communication list-key --name cdm-communication -g $rg --query primaryConnectionString -o tsv
$fromAddress = 'DoNotReply@67ee7f20-7b4b-4ba6-8d80-96458ff3392e.azurecomm.net'

az webapp config appsettings set -g $rg -n $api --settings `
    "AzureEmail__ConnectionString=$acsConnectionString" `
    "AzureEmail__FromAddress=$fromAddress" `
    --output none

# --- Verification ------------------------------------------------------------
Write-Host "`nApp settings de l'API :" -ForegroundColor Cyan
az webapp config appsettings list -g $rg -n $api --query "[?starts_with(name,'ImageStorage') || starts_with(name,'AzureEmail')].name" -o tsv

Write-Host "`nRedemarrage de l'API..." -ForegroundColor Cyan
az webapp restart -g $rg -n $api --output none
Write-Host "Termine. Teste un upload d'avatar puis un mail de reinitialisation." -ForegroundColor Green
