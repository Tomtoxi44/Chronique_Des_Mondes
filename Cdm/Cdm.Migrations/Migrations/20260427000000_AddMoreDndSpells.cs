// -----------------------------------------------------------------------
// <copyright file="20260427000000_AddMoreDndSpells.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Migrations.Migrations;

using Cdm.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
/// Idempotent migration: adds ~60 new D&amp;D 5e spells organised by level (0–9).
/// </summary>
[DbContext(typeof(MigrationsContext))]
[Migration("20260427000000_AddMoreDndSpells")]
public class AddMoreDndSpells : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── Cantrips (6 new) ──────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndSpells] WHERE [Name] = 'Trait de feu')
BEGIN
    INSERT INTO [dbo].[DndSpells] ([Name],[Level],[School],[Description],[CastingTime],[Range],[Duration],[DamageDice],[DamageType],[Classes],[Components],[RequiresConcentration],[IsRitual]) VALUES
    ('Trait de feu',        0, 'Évocation',     'Lance un trait enflammé sur une cible à distance.',                                            '1 action', '36 mètres', 'Instantané', '1d10', 'Feu',       '[""Wizard"",""Sorcerer""]',                  'VS',  0, 0),
    ('Aspersion acide',     0, 'Conjuration',   'Projette une bulle d''acide sur une ou deux créatures adjacentes.',                            '1 action', '18 mètres', 'Instantané', '1d6',  'Acide',     '[""Wizard"",""Sorcerer""]',                  'VS',  0, 0),
    ('Lumière',             0, 'Évocation',     'Touchez un objet : il émet de la lumière brillante sur 6 mètres pendant 1 heure.',             '1 action', 'Tactile',   '1 heure',    NULL,   NULL,        '[""Cleric"",""Druid"",""Wizard"",""Bard""]', 'VM',  0, 0),
    ('Thaumaturgie',        0, 'Transmutation', 'Créez un effet surnaturel mineur : voix forte, flamme tremblante, tremblement de sol.',        '1 action', '9 mètres',  '1 minute',   NULL,   NULL,        '[""Cleric""]',                               'V',   0, 0),
    ('Raillerie cinglante', 0, 'Enchantement',  'Vos insultes magiques infligent des dégâts et causent un désavantage à la prochaine attaque.', '1 action', '18 mètres', 'Instantané', '1d4',  'Psychique', '[""Bard""]',                                 'V',   0, 0),
    ('Lame des ombres',     0, 'Évocation',     'Rayon de force occulte frappant une cible ; peut être lancé plusieurs fois par niveau.',       '1 action', '36 mètres', 'Instantané', '1d10', 'Force',     '[""Warlock""]',                              'VS',  0, 0);
END");

        // ── Level 1 (11 new) ──────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndSpells] WHERE [Name] = 'Vague de tonnerre')
BEGIN
    INSERT INTO [dbo].[DndSpells] ([Name],[Level],[School],[Description],[CastingTime],[Range],[Duration],[DamageDice],[DamageType],[Classes],[Components],[RequiresConcentration],[IsRitual]) VALUES
    ('Vague de tonnerre',                   1, 'Évocation',     'Onde sonore dans un cube de 4,5 m ; repousse les créatures et objets.',                        '1 action',       'Soi (cube 4,5m)', 'Instantané',              '2d8', 'Tonnerre', '[""Cleric"",""Druid"",""Sorcerer"",""Wizard"",""Bard""]',  'VS',  0, 0),
    ('Entrelacs',                           1, 'Transmutation', 'Des lianes retiennent les créatures dans une zone de 6 m pendant 1 minute.',                   '1 action',       '27 mètres',       'Concentration, 1 minute', NULL,  NULL,       '[""Druid"",""Ranger""]',                                   'VS',  1, 0),
    ('Protection contre le mal et le bien', 1, 'Abjuration',    'Protège contre aberrations, célestes, élémentaires, fées, fiélons et morts-vivants.',          '1 action',       'Tactile',         'Concentration, 10 minutes', NULL, NULL,      '[""Cleric"",""Druid"",""Paladin"",""Wizard""]',            'VSM', 1, 0),
    ('Identification',                      1, 'Divination',    'Révèle les propriétés magiques d''un objet ou d''un sort affectant une créature.',             '1 minute',       'Tactile',         'Instantané',              NULL,  NULL,       '[""Bard"",""Wizard""]',                                    'VSM', 0, 1),
    ('Retraite expéditive',                 1, 'Transmutation', 'Votre Déplacement augmente ; vous pouvez sprinter en action bonus chaque round.',              '1 action bonus', 'Soi',             'Concentration, 10 minutes', NULL, NULL,      '[""Sorcerer"",""Warlock"",""Wizard""]',                    'V',   1, 0),
    ('Déguisement',                         1, 'Illusion',      'Modifiez votre apparence ainsi que vos vêtements pendant 1 heure.',                            '1 action',       'Soi',             '1 heure',                 NULL,  NULL,       '[""Bard"",""Sorcerer"",""Wizard""]',                       'VS',  0, 0),
    ('Mot de guérison',                     1, 'Évocation',     'Restaurez 1d4 + mod PV à une créature visible via une action bonus.',                          '1 action bonus', '18 mètres',       'Instantané',              '1d4', 'Soin',     '[""Bard"",""Cleric"",""Druid""]',                          'V',   0, 0),
    ('Châtiment divin',                     1, 'Évocation',     'Infligez 2d8 dégâts radieux supplémentaires à la prochaine attaque de corps à corps réussie.', '1 action',       'Soi',             'Instantané',              '2d8', 'Radiant',  '[""Paladin""]',                                            'V',   0, 0),
    ('Compréhension des langues',           1, 'Divination',    'Comprenez toutes les langues parlées ou écrites pendant 1 heure.',                             '1 action',       'Soi',             '1 heure',                 NULL,  NULL,       '[""Bard"",""Sorcerer"",""Warlock"",""Wizard""]',           'VSM', 0, 1),
    ('Brume',                               1, 'Conjuration',   'Créez un nuage de brouillard opaque de 6 m de rayon ; se déplace avec le vent.',               '1 action',       '27 mètres',       'Concentration, 1 heure',  NULL,  NULL,       '[""Druid"",""Ranger"",""Sorcerer"",""Wizard""]',           'V',   1, 0),
    ('Bouclier de la foi',                  1, 'Abjuration',    'Accordez à une créature un bonus de +2 à la CA grâce à un champ d''énergie divine.',           '1 action bonus', '18 mètres',       'Concentration, 10 minutes', NULL, NULL,      '[""Cleric"",""Paladin""]',                                 'VSM', 1, 0);
END");

        // ── Level 2 (12 new) ──────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndSpells] WHERE [Name] = 'Pas brumeux')
BEGIN
    INSERT INTO [dbo].[DndSpells] ([Name],[Level],[School],[Description],[CastingTime],[Range],[Duration],[DamageDice],[DamageType],[Classes],[Components],[RequiresConcentration],[IsRitual]) VALUES
    ('Pas brumeux',                  2, 'Conjuration',   'Téléportez-vous en action bonus à jusqu''à 9 mètres dans un espace libre visible.',      '1 action bonus', 'Soi',      'Instantané',              NULL,  NULL,        '[""Sorcerer"",""Warlock"",""Wizard""]',                              'V',   0, 0),
    ('Silence',                      2, 'Illusion',      'Zone de silence total de 6 m de rayon pendant 10 minutes ; aucun sort verbal possible.',  '1 action',       '45 mètres','Concentration, 10 minutes', NULL, NULL,        '[""Bard"",""Cleric"",""Ranger""]',                                   'VS',  1, 1),
    ('Aide',                         2, 'Abjuration',    'Augmentez le maximum et les PV actuels de 3 créatures de 5 points pendant 8 heures.',    '1 action',       '9 mètres', '8 heures',                NULL,  NULL,        '[""Bard"",""Cleric"",""Paladin"",""Ranger""]',                       'VSM', 0, 0),
    ('Amélioration de compétence',   2, 'Transmutation', 'Accordez l''avantage sur les jets de la caractéristique choisie pendant 1 heure.',       '1 action',       'Tactile',  'Concentration, 1 heure',  NULL,  NULL,        '[""Bard"",""Cleric"",""Druid"",""Sorcerer""]',                       'VSM', 1, 0),
    ('Immobilisation de personne',   2, 'Enchantement',  'Paralyse un humanoïde pendant 1 minute ; sauvegarde Sagesse chaque round.',              '1 action',       '18 mètres','Concentration, 1 minute', NULL,  NULL,        '[""Bard"",""Cleric"",""Druid"",""Sorcerer"",""Warlock"",""Wizard""]','VS',  1, 0),
    ('Rayon ardent',                 2, 'Évocation',     'Lancez trois rayons de feu (2d6 Feu chacun) sur une ou plusieurs cibles.',               '1 action',       '36 mètres','Instantané',              '2d6', 'Feu',       '[""Sorcerer"",""Wizard""]',                                          'VS',  0, 0),
    ('Cécité/Surdité',               2, 'Nécromancie',   'Rendez une créature aveugle ou sourde pendant 1 minute ; sauvegarde Constitution.',      '1 action',       '27 mètres','1 minute',                NULL,  NULL,        '[""Bard"",""Cleric"",""Sorcerer"",""Wizard""]',                      'V',   0, 0),
    ('Protection contre les poisons',2, 'Abjuration',    'Neutralise les poisons actifs et confère la résistance aux dégâts de poison (1 heure).', '1 action',       'Tactile',  '1 heure',                 NULL,  NULL,        '[""Cleric"",""Druid"",""Paladin"",""Ranger""]',                      'VS',  0, 0),
    ('Flèche acide de Melf',         2, 'Évocation',     'Flèche acide : 4d4 dégâts immédiats et 2d4 supplémentaires à la fin du prochain tour.', '1 action',       '27 mètres','Instantané',              '4d4', 'Acide',     '[""Wizard""]',                                                       'VSM', 0, 0),
    ('Croissance d''épines',         2, 'Transmutation', 'Zone de ronces infligeant 2d4 perforant à chaque créature qui s''y déplace.',            '1 action',       '45 mètres','Concentration, 10 minutes','2d4', 'Perforant', '[""Druid"",""Ranger""]',                                             'VSM', 1, 0),
    ('Fouet de l''esprit',           2, 'Enchantement',  'Craquement mental infligeant 3d6 dégâts psychiques et réduisant la réaction de la cible.','1 action',     '18 mètres','Instantané',              '3d6', 'Psychique', '[""Sorcerer"",""Wizard""]',                                          'V',   0, 0),
    ('Lien de gardien',              2, 'Abjuration',    'Lie deux créatures ; les dégâts subis par l''une sont partagés avec l''autre.',          '1 action',       'Tactile',  '1 heure',                 NULL,  NULL,        '[""Cleric"",""Paladin""]',                                           'VSM', 0, 0);
END");

        // ── Level 3 (8 new) ───────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndSpells] WHERE [Name] = 'Rappel à la vie')
BEGIN
    INSERT INTO [dbo].[DndSpells] ([Name],[Level],[School],[Description],[CastingTime],[Range],[Duration],[DamageDice],[DamageType],[Classes],[Components],[RequiresConcentration],[IsRitual]) VALUES
    ('Rappel à la vie',        3, 'Nécromancie',   'Ramenez une créature morte depuis moins d''1 minute à 1 PV.',                             '1 action',   'Tactile',       'Instantané',              NULL, NULL, '[""Cleric"",""Druid"",""Paladin"",""Ranger""]',                                              'VSM', 0, 0),
    ('Dissipation de la magie',3, 'Abjuration',    'Mettez fin aux sorts et effets magiques actifs sur une cible.',                           '1 action',   '36 mètres',     'Instantané',              NULL, NULL, '[""Bard"",""Cleric"",""Druid"",""Paladin"",""Sorcerer"",""Warlock"",""Wizard""]',         'VS',  0, 0),
    ('Vol',                    3, 'Transmutation', 'Accordez la capacité de voler à une créature touchée pendant 10 minutes.',                '1 action',   'Tactile',       'Concentration, 10 minutes', NULL, NULL, '[""Sorcerer"",""Warlock"",""Wizard""]',                                               'VSM', 1, 0),
    ('Hâte',                   3, 'Transmutation', 'Doublez la vitesse d''une créature, +2 CA, avantage Dex, action supplémentaire par round.','1 action',  '9 mètres',      'Concentration, 1 minute', NULL, NULL, '[""Sorcerer"",""Wizard""]',                                                           'VSM', 1, 0),
    ('Lenteur',                3, 'Transmutation', 'Ralentissez jusqu''à 6 créatures : vitesse divisée, malus aux attaques et à la CA.',      '1 action',   '27 mètres',     'Concentration, 1 minute', NULL, NULL, '[""Sorcerer"",""Wizard""]',                                                           'VSM', 1, 0),
    ('Terreur',                3, 'Illusion',      'Les créatures dans un cône de 9 m fuient en lâchant leurs objets.',                       '1 action',   'Soi (cône 9m)', 'Concentration, 1 minute', NULL, NULL, '[""Bard"",""Sorcerer"",""Warlock"",""Wizard""]',                                      'VS',  1, 0),
    ('Nuage malodorant',       3, 'Conjuration',   'Sphère de gaz nauséabond de 6 m : les créatures intoxiquées passent leur action.',        '1 action',   '27 mètres',     'Concentration, 1 minute', NULL, NULL, '[""Bard"",""Sorcerer"",""Wizard""]',                                                  'VSM', 1, 0),
    ('Clairvoyance',           3, 'Divination',    'Créez un capteur invisible en un lieu connu pour voir ou entendre à distance.',           '10 minutes', '1,5 km',        'Concentration, 10 minutes', NULL, NULL, '[""Bard"",""Cleric"",""Sorcerer"",""Wizard""]',                                   'VSM', 1, 0);
END");

        // ── Level 4 (7 new) ───────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndSpells] WHERE [Name] = 'Polymorphie')
BEGIN
    INSERT INTO [dbo].[DndSpells] ([Name],[Level],[School],[Description],[CastingTime],[Range],[Duration],[DamageDice],[DamageType],[Classes],[Components],[RequiresConcentration],[IsRitual]) VALUES
    ('Polymorphie',            4, 'Transmutation', 'Transformez une créature en une bête pendant 1 heure (concentration).',                  '1 action', '18 mètres', 'Concentration, 1 heure',  NULL,      NULL,             '[""Bard"",""Druid"",""Sorcerer"",""Wizard""]',              'VSM', 1, 0),
    ('Tempête de glace',       4, 'Évocation',     'Grêle et éclairs dans un cylindre : 2d8 contondant + 4d6 froid, zone glissante.',        '1 action', '90 mètres', 'Instantané',              '2d8+4d6', 'Contondant/Froid','[""Cleric"",""Druid"",""Sorcerer"",""Wizard""]',          'VSM', 0, 0),
    ('Porte dimensionnelle',   4, 'Conjuration',   'Téléportez-vous ainsi qu''un allié jusqu''à 150 m en un lieu familier ou visible.',      '1 action', '150 mètres','Instantané',              NULL,      NULL,             '[""Bard"",""Sorcerer"",""Warlock"",""Wizard""]',            'V',   0, 0),
    ('Mur de feu',             4, 'Évocation',     'Mur de flammes infligeant 5d8 dégâts de feu aux créatures traversant ou à l''intérieur.','1 action', '36 mètres', 'Concentration, 1 minute', '5d8',     'Feu',            '[""Druid"",""Sorcerer"",""Wizard""]',                       'VSM', 1, 0),
    ('Invisibilité améliorée', 4, 'Illusion',      'La cible reste invisible même lors d''attaques ou de lancement de sorts.',               '1 action', 'Tactile',   'Concentration, 1 minute', NULL,      NULL,             '[""Bard"",""Sorcerer"",""Wizard""]',                         'VS',  1, 0),
    ('Flétrissement',          4, 'Nécromancie',   'Drains de vie nécrotiques : 8d8 dégâts nécrotiques sur une cible.',                      '1 action', '9 mètres',  'Instantané',              '8d8',     'Nécrotique',     '[""Cleric"",""Druid"",""Sorcerer"",""Warlock"",""Wizard""]', 'VS',  0, 0),
    ('Bannissement',           4, 'Abjuration',    'Bannissez une créature vers son plan d''origine ou un demi-plan pendant 1 minute.',      '1 action', '18 mètres', 'Concentration, 1 minute', NULL,      NULL,             '[""Cleric"",""Paladin"",""Sorcerer"",""Warlock"",""Wizard""]','VSM', 1, 0);
END");

        // ── Level 5 (7 new) ───────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndSpells] WHERE [Name] = 'Cône de froid')
BEGIN
    INSERT INTO [dbo].[DndSpells] ([Name],[Level],[School],[Description],[CastingTime],[Range],[Duration],[DamageDice],[DamageType],[Classes],[Components],[RequiresConcentration],[IsRitual]) VALUES
    ('Cône de froid',            5, 'Évocation',    'Souffle gelé en cône de 18 m infligeant 8d8 dégâts de froid.',                                '1 action',  'Soi (cône 18m)', 'Instantané',              '8d8',     'Froid',      '[""Sorcerer"",""Wizard""]',                    'VSM', 0, 0),
    ('Immobilisation de monstre',5, 'Enchantement', 'Paralyse n''importe quelle créature (pas seulement humanoïde) pendant 1 minute.',             '1 action',  '18 mètres',      'Concentration, 1 minute', NULL,      NULL,         '[""Bard"",""Sorcerer"",""Warlock"",""Wizard""]', 'VS',  1, 0),
    ('Rappel des morts',         5, 'Nécromancie',  'Ramenez une créature morte depuis moins de 10 jours à la vie avec 1 PV.',                     '1 heure',   'Tactile',        'Instantané',              NULL,      NULL,         '[""Bard"",""Cleric"",""Paladin""]',             'VSM', 0, 0),
    ('Frappe de flammes',        5, 'Évocation',    'Colonne de feu divin : 4d6 Feu + 4d6 Radiant dans un cylindre de 3 m de rayon.',              '1 action',  '18 mètres',      'Instantané',              '4d6+4d6', 'Feu/Radiant','[""Cleric"",""Druid""]',                      'VS',  0, 0),
    ('Nuée d''insectes',         5, 'Conjuration',  'Nuage d''insectes piquants de 6 m de rayon : 4d10 perforant par round.',                      '1 action',  '90 mètres',      'Concentration, 10 minutes','4d10',    'Perforant',  '[""Cleric"",""Druid"",""Sorcerer""]',          'VSM', 1, 0),
    ('Sanctification',           5, 'Évocation',    'Consacrez une zone de 18 m de rayon protégeant contre les entités maléfiques.',               '24 heures', 'Tactile',        'Jusqu''à dissipation',    NULL,      NULL,         '[""Cleric""]',                                  'VSM', 0, 0),
    ('Passe-muraille',           5, 'Transmutation','Créez un passage dans un mur de bois, plâtre ou pierre de 1,5 m de profondeur.',              '1 action',  'Tactile',        '1 heure',                 NULL,      NULL,         '[""Wizard""]',                                  'VS',  0, 0);
END");

        // ── Levels 6–9 (9 new) ────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndSpells] WHERE [Name] = 'Globe d''invulnérabilité')
BEGIN
    INSERT INTO [dbo].[DndSpells] ([Name],[Level],[School],[Description],[CastingTime],[Range],[Duration],[DamageDice],[DamageType],[Classes],[Components],[RequiresConcentration],[IsRitual]) VALUES
    ('Globe d''invulnérabilité',       6, 'Abjuration',    'Barrière sphérique empêchant les sorts de niveau 5 et inférieurs de pénétrer.',           '1 action',  'Soi',      'Concentration, 1 minute', NULL,      NULL,        '[""Sorcerer"",""Wizard""]',                          'VSM', 1, 0),
    ('Désintégration',                 6, 'Transmutation', 'Rayon destructeur : 10d6+40 dégâts de force ; la cible est réduite en poussière.',        '1 action',  '18 mètres','Instantané',              '10d6+40', 'Force',     '[""Sorcerer"",""Wizard""]',                          'VSM', 0, 0),
    ('Cercle de mort',                 6, 'Nécromancie',   'Sphère nécrotique de 18 m de rayon infligeant 8d6 dégâts nécrotiques.',                  '1 action',  '60 mètres','Instantané',              '8d6',     'Nécrotique','[""Sorcerer"",""Warlock"",""Wizard""]',              'VSM', 0, 0),
    ('Résurrection',                   7, 'Nécromancie',   'Ramenez un mort depuis au plus 100 ans à la vie avec la totalité de ses PV.',            '1 heure',   'Tactile',  'Instantané',              NULL,      NULL,        '[""Bard"",""Cleric""]',                              'VSM', 0, 0),
    ('Image projetée',                 7, 'Illusion',      'Projetez une image illusoire de vous-même à 750 km pendant 1 jour (concentration).',     '1 action',  '750 km',   'Concentration, 1 jour',   NULL,      NULL,        '[""Bard"",""Wizard""]',                              'VSM', 1, 0),
    ('Mot de puissance: Étourdissant', 8, 'Enchantement',  'Prononcez un mot de pouvoir étourdissant une créature ayant 150 PV ou moins.',          '1 action',  '18 mètres','Variable',                NULL,      NULL,        '[""Bard"",""Sorcerer"",""Warlock"",""Wizard""]',     'V',   0, 0),
    ('Domination de monstre',          8, 'Enchantement',  'Contrôlez télépatiquement une créature quelconque pendant 1 heure (concentration).',    '1 action',  '18 mètres','Concentration, 1 heure',  NULL,      NULL,        '[""Bard"",""Sorcerer"",""Warlock"",""Wizard""]',     'VS',  1, 0),
    ('Pressentiment',                  9, 'Divination',    'Pendant 8 heures : avantage aux jets d''attaque, de sauvegarde et de compétence.',       '1 minute',  'Tactile',  '8 heures',                NULL,      NULL,        '[""Bard"",""Druid"",""Warlock"",""Wizard""]',        'VSM', 0, 0),
    ('Arrêt du temps',                 9, 'Transmutation', 'Arrêtez le temps : agissez seul pendant 1d4+1 rounds consécutifs.',                     '1 action',  'Soi',      'Variable',                NULL,      NULL,        '[""Sorcerer"",""Wizard""]',                          'V',   0, 0);
END");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DELETE FROM [dbo].[DndSpells] WHERE [Name] IN (
    'Trait de feu', 'Aspersion acide', 'Lumière', 'Thaumaturgie', 'Raillerie cinglante', 'Lame des ombres',
    'Vague de tonnerre', 'Entrelacs', 'Protection contre le mal et le bien', 'Identification',
    'Retraite expéditive', 'Déguisement', 'Mot de guérison', 'Châtiment divin',
    'Compréhension des langues', 'Brume', 'Bouclier de la foi',
    'Pas brumeux', 'Silence', 'Aide', 'Amélioration de compétence', 'Immobilisation de personne',
    'Rayon ardent', 'Cécité/Surdité', 'Protection contre les poisons', 'Flèche acide de Melf',
    'Croissance d''épines', 'Fouet de l''esprit', 'Lien de gardien',
    'Rappel à la vie', 'Dissipation de la magie', 'Vol', 'Hâte', 'Lenteur',
    'Terreur', 'Nuage malodorant', 'Clairvoyance',
    'Polymorphie', 'Tempête de glace', 'Porte dimensionnelle', 'Mur de feu',
    'Invisibilité améliorée', 'Flétrissement', 'Bannissement',
    'Cône de froid', 'Immobilisation de monstre', 'Rappel des morts',
    'Frappe de flammes', 'Nuée d''insectes', 'Sanctification', 'Passe-muraille',
    'Globe d''invulnérabilité', 'Désintégration', 'Cercle de mort',
    'Résurrection', 'Image projetée', 'Mot de puissance: Étourdissant',
    'Domination de monstre', 'Pressentiment', 'Arrêt du temps'
);");
    }
}
