// -----------------------------------------------------------------------
// <copyright file="20260426110000_AddDndBackgroundsAndSkills.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Migrations.Migrations;

using Cdm.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
/// Idempotent migration: adds DndBackgrounds and DndSkills tables,
/// adds AvailableSkills column to DndClasses,
/// and seeds all reference data.
/// </summary>
[DbContext(typeof(MigrationsContext))]
[Migration("20260426110000_AddDndBackgroundsAndSkills")]
public class AddDndBackgroundsAndSkills : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── DndBackgrounds ────────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[DndBackgrounds]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DndBackgrounds] (
        [Id]                 int NOT NULL IDENTITY,
        [Name]               nvarchar(100) NOT NULL,
        [Description]        nvarchar(1000) NULL,
        [SkillProficiencies] nvarchar(max) NULL,
        [ToolProficiencies]  nvarchar(max) NULL,
        [Languages]          nvarchar(200) NULL,
        [Feature]            nvarchar(200) NULL,
        [FeatureDescription] nvarchar(max) NULL,
        [IsActive]           bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_DndBackgrounds] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_DndBackgrounds_Name] ON [dbo].[DndBackgrounds] ([Name]);
END");

        // ── DndSkills ─────────────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[DndSkills]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DndSkills] (
        [Id]            int NOT NULL IDENTITY,
        [Name]          nvarchar(100) NOT NULL,
        [LinkedAbility] nvarchar(50) NOT NULL,
        [Description]   nvarchar(500) NULL,
        [IsActive]      bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_DndSkills] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_DndSkills_Name] ON [dbo].[DndSkills] ([Name]);
    CREATE INDEX [IX_DndSkills_LinkedAbility] ON [dbo].[DndSkills] ([LinkedAbility]);
END");

        // ── DndClasses: add AvailableSkills ───────────────────────────────────
        migrationBuilder.Sql(@"
IF COL_LENGTH('[dbo].[DndClasses]', 'AvailableSkills') IS NULL
BEGIN
    ALTER TABLE [dbo].[DndClasses] ADD [AvailableSkills] nvarchar(max) NULL;
END");

        // ── Seed DndBackgrounds ───────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndBackgrounds] WHERE [Name] = 'Acolyte')
BEGIN
    INSERT INTO [dbo].[DndBackgrounds] ([Name],[Description],[SkillProficiencies],[ToolProficiencies],[Languages],[Feature],[FeatureDescription]) VALUES
    ('Acolyte', 'Vous avez passé votre vie au service d''un temple, apprenant à manier les mystères du divin.', 
     '[""Perspicacité"",""Religion""]', NULL, '2 langues supplémentaires au choix',
     'Abri des fidèles', 'Vous commandez le respect de ceux qui partagent votre foi. Vous et vos compagnons pouvez obtenir hébergement et soins gratuitement dans les temples de votre panthéon.'),
    
    ('Charlatan', 'Vous avez toujours eu des facilités pour faire croire n''importe quoi aux gens.', 
     '[""Tromperie"",""Persuasion""]', '[""Kit de déguisement"",""Kit de contrefaçon""]', NULL,
     'Fausse identité', 'Vous possédez une deuxième identité secrète, comprenant documentation et contacts.'),
    
    ('Criminel', 'Vous avez un passé dans l''activité criminelle.', 
     '[""Discrétion"",""Tromperie""]', '[""Outils de voleur"",""Un type de jeu""]', NULL,
     'Contact criminel', 'Vous avez un contact fiable dans le monde criminel, un intermédiaire vers des réseaux criminels.'),
    
    ('Artiste', 'Vous vous épanouissez dans les arts du spectacle.', 
     '[""Acrobaties"",""Représentation""]', '[""Instruments de musique (2 au choix)""]', NULL,
     'Par amour de la scène', 'Vous pouvez toujours trouver un endroit pour vous produire et loger gracieusement.'),
    
    ('Héros du Peuple', 'Vous venez d''un milieu humble mais vous êtes destiné à plus.', 
     '[""Dressage"",""Survie""]', '[""Un type d''''outil artisan"",""Véhicules terrestres""]', NULL,
     'Hospitalité rustique', 'Les gens du commun vous apportent leur soutien, vous hébergent et vous nourrissent.'),
    
    ('Artisan de Guilde', 'Vous êtes membre d''une guilde artisanale.', 
     '[""Perspicacité"",""Persuasion""]', '[""Outils d''''artisan (type au choix)""]', '1 langue supplémentaire',
     'Appartenance à la guilde', 'La guilde fournit assistance, hébergement et soutien légal à ses membres.'),
    
    ('Ermite', 'Vous avez vécu en reclus, dans la méditation et l''isolement.', 
     '[""Médecine"",""Religion""]', '[""Kit de guérisseur"",""Herboristerie""]', '1 langue supplémentaire',
     'Découverte', 'Votre isolement vous a révélé une vérité puissante ou un secret unique.'),
    
    ('Noble', 'Vous comprenez la richesse, le pouvoir et les privilèges.', 
     '[""Histoire"",""Persuasion""]', '[""Un type de jeu""]', '1 langue supplémentaire',
     'Privilège de la noblesse', 'Votre rang vous permet d''accéder aux espaces nobles et de rencontrer hauts responsables.'),
    
    ('Baroudeur', 'Vous avez grandi dans les terres sauvages.', 
     '[""Athlétisme"",""Survie""]', '[""Instruments de musique (1 au choix)"",""Véhicules terrestres""]', '1 langue supplémentaire',
     'Marcheur des terres sauvages', 'Vous avez une excellente mémoire des cartes et pouvez vivre du territoire.'),
    
    ('Sage', 'Vous avez passé des années à apprendre les secrets du multivers.', 
     '[""Arcanes"",""Histoire""]', NULL, '2 langues supplémentaires au choix',
     'Chercheur', 'Vous savez toujours où trouver des informations et qui consulter en matière de savoir.'),
    
    ('Marin', 'Vous avez navigué des années sur les mers.', 
     '[""Athlétisme"",""Perception""]', '[""Instruments de musique (1 au choix)"",""Véhicules nautiques""]', NULL,
     'Passé de marin', 'Vous pouvez vous engager sur tout navire comme membre d''équipage.'),
    
    ('Soldat', 'Vous avez servi dans une armée organisée, apprenant la rigueur militaire.', 
     '[""Athlétisme"",""Intimidation""]', '[""Un type de jeu"",""Véhicules terrestres""]', NULL,
     'Rang militaire', 'Votre rang vous confère autorité et respect auprès des soldats.'),
    
    ('Gamin des Rues', 'Vous avez grandi dans les rues, développant instinct de survie et débrouillardise.', 
     '[""Discrétion"",""Escamotage""]', '[""Outils de déguisement"",""Outils de voleur""]', NULL,
     'Secrets de la ville', 'Vous connaissez les passages secrets de toute grande ville.');
END");

        // ── Seed DndSkills ────────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndSkills] WHERE [Name] = 'Acrobaties')
BEGIN
    INSERT INTO [dbo].[DndSkills] ([Name],[LinkedAbility],[Description]) VALUES
    ('Acrobaties', 'Dexterity', 'Permet d''effectuer des actions acrobatiques, des cabrioles, et d''éviter les chutes.'),
    ('Arcanes', 'Intelligence', 'Connaissance des sorts, objets magiques, plans, symboles et pratiques magiques.'),
    ('Athlétisme', 'Strength', 'Couvrir des prouesses d''escalade, sauts, natation et autres exploits physiques.'),
    ('Discrétion', 'Dexterity', 'Tenter de se cacher, se déplacer silencieusement ou passer inaperçu.'),
    ('Dressage', 'Wisdom', 'Calmer des animaux, monter des montures, et soigner des bêtes sauvages.'),
    ('Escamotage', 'Dexterity', 'Dérober des objets, dissimuler des choses sur soi ou jouer des tours de passe-passe.'),
    ('Histoire', 'Intelligence', 'Rappel de connaissances historiques, légendaires, sur les organisations et les royaumes.'),
    ('Intimidation', 'Charisma', 'Influencer les autres par la menace, la violence ou la peur.'),
    ('Investigation', 'Intelligence', 'Chercher des indices, analyser des preuves, et déduire la vérité.'),
    ('Médecine', 'Wisdom', 'Stabiliser les mourants, diagnostiquer les maladies et comprendre les blessures.'),
    ('Nature', 'Intelligence', 'Connaissance des plantes, animaux, météo, cycles naturels et terrains.'),
    ('Perception', 'Wisdom', 'Repérer, entendre ou détecter quelque chose grâce aux sens.'),
    ('Perspicacité', 'Wisdom', 'Déterminer les intentions d''une créature ou déceler des mensonges.'),
    ('Persuasion', 'Charisma', 'Influencer les autres de manière sincère, diplomatique ou bienveillante.'),
    ('Religion', 'Intelligence', 'Connaissance des divinités, rites, prières, symboles et hiérarchies religieuses.'),
    ('Représentation', 'Charisma', 'Se produire en public avec chant, danse, théâtre, instruments ou autre art.'),
    ('Survie', 'Wisdom', 'Suivre des pistes, trouver nourriture, prévoir la météo et s''orienter en pleine nature.'),
    ('Tromperie', 'Charisma', 'Cacher la vérité verbalement ou par des gestes, dissimuler des objets et mentir.');
END");

        // ── Update AvailableSkills for each class ─────────────────────────────
        migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [dbo].[DndClasses] WHERE [Name] = 'Guerrier' AND [AvailableSkills] IS NULL)
BEGIN
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Acrobaties"",""Athlétisme"",""Histoire"",""Perspicacité"",""Intimidation"",""Perception"",""Survie""]' WHERE [Name] = 'Guerrier';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Dressage"",""Intimidation"",""Nature"",""Perception"",""Survie"",""Athlétisme""]' WHERE [Name] = 'Barbare';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Athlétisme"",""Perspicacité"",""Intimidation"",""Médecine"",""Persuasion"",""Religion""]' WHERE [Name] = 'Paladin';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Dressage"",""Discrétion"",""Investigation"",""Nature"",""Perception"",""Survie"",""Athlétisme"",""Perspicacité""]' WHERE [Name] = 'Rôdeur';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Acrobaties"",""Athlétisme"",""Discrétion"",""Tromperie"",""Perspicacité"",""Intimidation"",""Investigation"",""Perception"",""Représentation"",""Persuasion"",""Escamotage""]' WHERE [Name] = 'Roublard';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Acrobaties"",""Athlétisme"",""Histoire"",""Perspicacité"",""Religion"",""Discrétion""]' WHERE [Name] = 'Moine';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Arcanes"",""Histoire"",""Perspicacité"",""Investigation"",""Médecine"",""Religion""]' WHERE [Name] = 'Magicien';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Arcanes"",""Tromperie"",""Perspicacité"",""Intimidation"",""Persuasion"",""Religion""]' WHERE [Name] = 'Ensorceleur';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Arcanes"",""Tromperie"",""Histoire"",""Intimidation"",""Investigation"",""Nature"",""Religion""]' WHERE [Name] = 'Occultiste';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Histoire"",""Perspicacité"",""Médecine"",""Persuasion"",""Religion""]' WHERE [Name] = 'Clerc';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Arcanes"",""Dressage"",""Perspicacité"",""Médecine"",""Nature"",""Perception"",""Religion"",""Survie""]' WHERE [Name] = 'Druide';
    UPDATE [dbo].[DndClasses] SET [AvailableSkills] = '[""Acrobaties"",""Dressage"",""Arcanes"",""Histoire"",""Perspicacité"",""Investigation"",""Nature"",""Perception"",""Représentation"",""Persuasion"",""Religion"",""Tromperie""]' WHERE [Name] = 'Barde';
END");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP TABLE IF EXISTS [dbo].[DndBackgrounds];");
        migrationBuilder.Sql(@"DROP TABLE IF EXISTS [dbo].[DndSkills];");
        migrationBuilder.Sql(@"
IF COL_LENGTH('[dbo].[DndClasses]', 'AvailableSkills') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[DndClasses] DROP COLUMN [AvailableSkills];
END");
    }
}
