// -----------------------------------------------------------------------
// <copyright file="20260422090000_AddDnd5eTablesAndCharacterBase.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Migrations.Migrations;

using Cdm.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
/// Idempotent migration: adds D&amp;D 5e reference tables, IsBaseCharacter/SourceCharacterId on Characters,
/// and GameSpecificData on NonPlayerCharacters.
/// Seeds core D&amp;D 5e reference data (races, classes, items, spells, monster templates).
/// </summary>
[DbContext(typeof(MigrationsContext))]
[Migration("20260422090000_AddDnd5eTablesAndCharacterBase")]
public class AddDnd5eTablesAndCharacterBase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── Characters: add IsBaseCharacter + SourceCharacterId ──────────────
        migrationBuilder.Sql(@"
IF COL_LENGTH('[dbo].[Characters]', 'IsBaseCharacter') IS NULL
BEGIN
    ALTER TABLE [dbo].[Characters] ADD [IsBaseCharacter] bit NOT NULL DEFAULT 0;
    CREATE INDEX [IX_Characters_IsBaseCharacter] ON [dbo].[Characters] ([IsBaseCharacter]);
END");

        migrationBuilder.Sql(@"
IF COL_LENGTH('[dbo].[Characters]', 'SourceCharacterId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Characters] ADD [SourceCharacterId] int NULL;
    ALTER TABLE [dbo].[Characters] ADD CONSTRAINT [FK_Characters_Characters_SourceCharacterId]
        FOREIGN KEY ([SourceCharacterId]) REFERENCES [dbo].[Characters]([Id]) ON DELETE NO ACTION;
    CREATE INDEX [IX_Characters_SourceCharacterId] ON [dbo].[Characters] ([SourceCharacterId]);
END");

        // ── NonPlayerCharacters: add GameSpecificData ─────────────────────────
        migrationBuilder.Sql(@"
IF COL_LENGTH('[dbo].[NonPlayerCharacters]', 'GameSpecificData') IS NULL
BEGIN
    ALTER TABLE [dbo].[NonPlayerCharacters] ADD [GameSpecificData] nvarchar(max) NULL;
END");

        // ── DndRaces ──────────────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[DndRaces]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DndRaces] (
        [Id]                 int NOT NULL IDENTITY,
        [Name]               nvarchar(100) NOT NULL,
        [Description]        nvarchar(1000) NULL,
        [Speed]              int NOT NULL DEFAULT 30,
        [StatBonuses]        nvarchar(max) NULL,
        [Traits]             nvarchar(max) NULL,
        [Subraces]           nvarchar(max) NULL,
        [SubraceStatBonuses] nvarchar(max) NULL,
        [IsActive]           bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_DndRaces] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_DndRaces_Name] ON [dbo].[DndRaces] ([Name]);
END");

        // ── DndClasses ────────────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[DndClasses]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DndClasses] (
        [Id]               int NOT NULL IDENTITY,
        [Name]             nvarchar(100) NOT NULL,
        [Description]      nvarchar(1000) NULL,
        [HitDie]           int NOT NULL,
        [IsSpellcaster]    bit NOT NULL DEFAULT 0,
        [PrimaryAbilities] nvarchar(max) NULL,
        [SavingThrows]     nvarchar(max) NULL,
        [Subclasses]       nvarchar(max) NULL,
        [IsActive]         bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_DndClasses] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_DndClasses_Name] ON [dbo].[DndClasses] ([Name]);
END");

        // ── DndItems ──────────────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[DndItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DndItems] (
        [Id]             int NOT NULL IDENTITY,
        [Name]           nvarchar(150) NOT NULL,
        [Category]       nvarchar(50) NOT NULL,
        [Description]    nvarchar(1000) NULL,
        [DamageDice]     nvarchar(20) NULL,
        [DamageType]     nvarchar(50) NULL,
        [WeaponRange]    nvarchar(20) NULL,
        [ArmorClassBonus] int NULL,
        [Weight]         decimal(8,2) NULL,
        [CostGp]         decimal(10,2) NULL,
        [Properties]     nvarchar(max) NULL,
        [HealingDice]    nvarchar(20) NULL,
        [IsActive]       bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_DndItems] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_DndItems_Category] ON [dbo].[DndItems] ([Category]);
    CREATE INDEX [IX_DndItems_Name] ON [dbo].[DndItems] ([Name]);
END");

        // ── DndSpells ─────────────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[DndSpells]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DndSpells] (
        [Id]                    int NOT NULL IDENTITY,
        [Name]                  nvarchar(150) NOT NULL,
        [Level]                 int NOT NULL DEFAULT 0,
        [School]                nvarchar(50) NULL,
        [Description]           nvarchar(max) NULL,
        [CastingTime]           nvarchar(100) NULL,
        [Range]                 nvarchar(50) NULL,
        [Duration]              nvarchar(100) NULL,
        [DamageDice]            nvarchar(30) NULL,
        [DamageType]            nvarchar(50) NULL,
        [Classes]               nvarchar(max) NULL,
        [Components]            nvarchar(10) NULL,
        [RequiresConcentration] bit NOT NULL DEFAULT 0,
        [IsRitual]              bit NOT NULL DEFAULT 0,
        [IsActive]              bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_DndSpells] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_DndSpells_Level] ON [dbo].[DndSpells] ([Level]);
    CREATE INDEX [IX_DndSpells_Name] ON [dbo].[DndSpells] ([Name]);
    CREATE INDEX [IX_DndSpells_School] ON [dbo].[DndSpells] ([School]);
END");

        // ── DndMonsterTemplates ───────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[DndMonsterTemplates]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DndMonsterTemplates] (
        [Id]               int NOT NULL IDENTITY,
        [Name]             nvarchar(150) NOT NULL,
        [MonsterType]      nvarchar(50) NULL,
        [ChallengeRating]  nvarchar(10) NULL,
        [Description]      nvarchar(max) NULL,
        [HitPoints]        int NOT NULL DEFAULT 0,
        [HitDice]          nvarchar(20) NULL,
        [ArmorClass]       int NOT NULL DEFAULT 10,
        [Speed]            int NOT NULL DEFAULT 30,
        [Strength]         int NOT NULL DEFAULT 10,
        [Dexterity]        int NOT NULL DEFAULT 10,
        [Constitution]     int NOT NULL DEFAULT 10,
        [Intelligence]     int NOT NULL DEFAULT 10,
        [Wisdom]           int NOT NULL DEFAULT 10,
        [Charisma]         int NOT NULL DEFAULT 10,
        [Actions]          nvarchar(max) NULL,
        [SpecialAbilities] nvarchar(max) NULL,
        [Alignment]        nvarchar(50) NULL,
        [IsActive]         bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_DndMonsterTemplates] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_DndMonsterTemplates_MonsterType] ON [dbo].[DndMonsterTemplates] ([MonsterType]);
    CREATE INDEX [IX_DndMonsterTemplates_ChallengeRating] ON [dbo].[DndMonsterTemplates] ([ChallengeRating]);
    CREATE INDEX [IX_DndMonsterTemplates_Name] ON [dbo].[DndMonsterTemplates] ([Name]);
END");

        // ═════════════════════════ SEED DATA ══════════════════════════════════

        // ── Races (PHB core + some common) ────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndRaces] WHERE [Name] = 'Humain')
BEGIN
    INSERT INTO [dbo].[DndRaces] ([Name],[Description],[Speed],[StatBonuses],[Traits],[Subraces],[SubraceStatBonuses]) VALUES
    ('Humain', 'Les humains sont les plus répandus des peuples mortels et sont connus pour leur polyvalence.', 30,
     '{""Strength"":1,""Dexterity"":1,""Constitution"":1,""Intelligence"":1,""Wisdom"":1,""Charisma"":1}',
     '[""Compétence supplémentaire"",""Aptitude supplémentaire""]', NULL, NULL),

    ('Haut-Elfe', 'Les hauts-elfes valorisent la magie et la sagesse au-dessus de tout.', 30,
     '{""Dexterity"":2}',
     '[""Vision dans le noir"",""Ascendance féerique"",""Résistance au sommeil magique"",""Entraînement aux armes elfiques"",""Tour de magie""]',
     '[""Haut-Elfe""]',
     '{""Haut-Elfe"":{""Intelligence"":1}}'),

    ('Elfe des Bois', 'Les elfes des bois ont une rapidité et une furtivité naturelle.', 35,
     '{""Dexterity"":2}',
     '[""Vision dans le noir"",""Ascendance féerique"",""Entraînement aux armes elfiques"",""Pas léger""]',
     '[""Elfe des Bois""]',
     '{""Elfe des Bois"":{""Wisdom"":1}}'),

    ('Nain des Collines', 'Les nains des collines ont une endurance et une sagesse pratique remarquables.', 25,
     '{""Constitution"":2}',
     '[""Vision dans le noir"",""Résistance aux poisons"",""Résilience naine"",""Maîtrise des outils""]',
     '[""Nain des Collines""]',
     '{""Nain des Collines"":{""Wisdom"":1}}'),

    ('Nain des Montagnes', 'Les nains des montagnes sont forts et robustes, entraînés au combat.', 25,
     '{""Constitution"":2}',
     '[""Vision dans le noir"",""Résistance aux poisons"",""Formation aux armures naines"",""Maîtrise des outils""]',
     '[""Nain des Montagnes""]',
     '{""Nain des Montagnes"":{""Strength"":2}}'),

    ('Halfelin Pied-Léger', 'Les halfelins sont agiles et furtifs, passant facilement inaperçus.', 25,
     '{""Dexterity"":2}',
     '[""Chanceux"",""Vaillance"",""Agilité halfeline""]',
     '[""Pied-Léger""]',
     '{""Pied-Léger"":{""Charisma"":1}}'),

    ('Demi-Orque', 'Les demi-orques portent la force et la férocité de leurs ancêtres orques.', 30,
     '{""Strength"":2,""Constitution"":1}',
     '[""Vision dans le noir"",""Menaçant"",""Endurance implacable"",""Attaques sauvages""]', NULL, NULL),

    ('Tieffelin', 'Portant l''héritage d''un pacte infernal, les tieffelins font face à des préjugés mais possèdent des pouvoirs innés.', 30,
     '{""Intelligence"":1,""Charisma"":2}',
     '[""Vision dans le noir"",""Résistance infernale"",""Héritage infernal""]', NULL, NULL),

    ('Drakéide', 'Fils de dragons, les drakéides portent avec fierté un héritage draconique.', 30,
     '{""Strength"":2,""Charisma"":1}',
     '[""Ascendance draconique"",""Souffle du dragon"",""Résistance des ancêtres""]', NULL, NULL),

    ('Demi-Elfe', 'Les demi-elfes combinent la grâce elfe et l''adaptabilité humaine.', 30,
     '{""Charisma"":2}',
     '[""Vision dans le noir"",""Ascendance féerique"",""Polyvalence des compétences"",""Deux points de caracteristique supplémentaires""]', NULL, NULL),

    ('Gnome des Roches', 'Les gnomes des roches sont des inventeurs et tinkerers ingénieux.', 25,
     '{""Intelligence"":2,""Constitution"":1}',
     '[""Vision dans le noir"",""Ruse gnomique"",""Connaisseur des artifices""]', NULL, NULL);
END");

        // ── Classes ───────────────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndClasses] WHERE [Name] = 'Guerrier')
BEGIN
    INSERT INTO [dbo].[DndClasses] ([Name],[Description],[HitDie],[IsSpellcaster],[PrimaryAbilities],[SavingThrows],[Subclasses]) VALUES
    ('Guerrier', 'Maître des armes et des armures, le guerrier est l''archétype du combattant.', 10, 0,
     '[""Strength"",""Constitution""]', '[""Strength"",""Constitution""]',
     '[""Champion"",""Maître de Bataille"",""Chevalier Occulte""]'),

    ('Barbare', 'Un guerrier sauvage qui entre en rage furieuse au combat.', 12, 0,
     '[""Strength"",""Constitution""]', '[""Strength"",""Constitution""]',
     '[""Berserker"",""Guerrier Totem"",""Âme Fanatique""]'),

    ('Paladin', 'Un guerrier sacré lié par serment à des idéaux de justice et de vertu.', 10, 1,
     '[""Strength"",""Charisma""]', '[""Wisdom"",""Charisma""]',
     '[""Serment de Dévotion"",""Serment des Anciens"",""Serment de Vengeance""]'),

    ('Rôdeur', 'Un chasseur et un guide expert des terres sauvages.', 10, 1,
     '[""Dexterity"",""Wisdom""]', '[""Strength"",""Dexterity""]',
     '[""Chasseur"",""Maître des Bêtes"",""Rodeur de l''Horizon""]'),

    ('Roublard', 'Un expert en discrétion, tromperie et attaques précises.', 8, 0,
     '[""Dexterity""]', '[""Dexterity"",""Intelligence""]',
     '[""Arnaqueur"",""Assassin"",""Trickster Arcanique""]'),

    ('Moine', 'Un maître des arts martiaux qui canalise une énergie mystique appelée ki.', 8, 0,
     '[""Dexterity"",""Wisdom""]', '[""Strength"",""Dexterity""]',
     '[""Voie de la Main Ouverte"",""Voie de l''Ombre"",""Voie des Quatre Éléments""]'),

    ('Magicien', 'Un érudit de la magie arcane qui lance des sorts de puissance dévastatrice.', 6, 1,
     '[""Intelligence""]', '[""Intelligence"",""Wisdom""]',
     '[""École d''Abjuration"",""École d''Évocation"",""École d''Illusion"",""École de Nécromancie"",""École de Transmutation""]'),

    ('Ensorceleur', 'Un lanceur de sorts dont les pouvoirs magiques proviennent d''une lignée surnaturelle.', 6, 1,
     '[""Charisma""]', '[""Constitution"",""Charisma""]',
     '[""Magie Draconique"",""Âme Sauvage"",""Origine Divine""]'),

    ('Occultiste', 'Un utilisateur de magie dont les pouvoirs découlent d''un pacte avec un être surpuissant.', 8, 1,
     '[""Charisma""]', '[""Wisdom"",""Charisma""]',
     '[""L''Archifée"",""Le Fiélon"",""Le Grand Ancien""]'),

    ('Clerc', 'Un serviteur divin qui canalise le pouvoir de son dieu.', 8, 1,
     '[""Wisdom"",""Charisma""]', '[""Wisdom"",""Charisma""]',
     '[""Domaine de la Vie"",""Domaine de la Lumière"",""Domaine de la Guerre"",""Domaine des Tempêtes"",""Domaine de la Tromperie""]'),

    ('Druide', 'Un gardien de la nature qui puise sa magie dans les forces naturelles du monde.', 8, 1,
     '[""Wisdom""]', '[""Intelligence"",""Wisdom""]',
     '[""Cercle de la Lune"",""Cercle des Terres""]'),

    ('Barde', 'Un artiste dont la magie provient de la musique, de la poésie et de la danse.', 8, 1,
     '[""Charisma""]', '[""Dexterity"",""Charisma""]',
     '[""Collège du Savoir"",""Collège de la Vaillance""]');
END");

        // ── Items – Weapons ────────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndItems] WHERE [Name] = 'Dague')
BEGIN
    INSERT INTO [dbo].[DndItems] ([Name],[Category],[Description],[DamageDice],[DamageType],[WeaponRange],[Weight],[CostGp],[Properties]) VALUES
    ('Dague',        'Weapon', 'Arme légère et lançable.',           '1d4',  'Perforant',  'Melee',  1.0, 2.0,   '[""Finesse"",""Légère"",""Lançable (portée 6/18m)""]'),
    ('Épée courte',  'Weapon', 'Arme légère tenue à une main.',      '1d6',  'Perforant',  'Melee',  2.0, 10.0,  '[""Finesse"",""Légère""]'),
    ('Épée longue',  'Weapon', 'Arme de chevalier polyvalente.',     '1d8',  'Tranchant',  'Melee',  3.0, 15.0,  '[""Polyvalente (1d10)""]'),
    ('Épée à deux mains', 'Weapon', 'Arme lourde à deux mains.',    '2d6',  'Tranchant',  'Melee',  6.0, 50.0,  '[""Lourde"",""À deux mains""]'),
    ('Hache de guerre', 'Weapon', 'Hache polyvalente puissante.',    '1d8',  'Tranchant',  'Melee',  2.0, 10.0,  '[""Polyvalente (1d10)""]'),
    ('Grande hache', 'Weapon', 'Hache lourde à deux mains.',        '1d12', 'Tranchant',  'Melee',  7.0, 30.0,  '[""Lourde"",""À deux mains""]'),
    ('Hachette',     'Weapon', 'Petite hache lançable.',             '1d6',  'Tranchant',  'Melee',  2.0, 5.0,   '[""Légère"",""Lançable (portée 6/18m)""]'),
    ('Lance',        'Weapon', 'Arme d''hast à portée étendue.',     '1d6',  'Perforant',  'Melee',  3.0, 1.0,   '[""Lançable (portée 6/18m)"",""Polyvalente (1d8)""]'),
    ('Marteau de guerre', 'Weapon', 'Marteau solide polyvalent.',   '1d8',  'Contondant', 'Melee',  2.0, 15.0,  '[""Polyvalente (1d10)""]'),
    ('Masse d''armes', 'Weapon', 'Masse simple contondante.',        '1d6',  'Contondant', 'Melee',  4.0, 5.0,   '[]'),
    ('Gourdin',      'Weapon', 'Arme simple improvisée.',           '1d4',  'Contondant', 'Melee',  2.0, 0.1,   '[""Légère""]'),
    ('Bâton',        'Weapon', 'Bâton polyvalent.',                 '1d6',  'Contondant', 'Melee',  4.0, 0.2,   '[""Polyvalente (1d8)""]'),
    ('Rapière',      'Weapon', 'Épée fine et précise.',              '1d8',  'Perforant',  'Melee',  2.0, 25.0,  '[""Finesse""]'),
    ('Arc court',    'Weapon', 'Arc compact maniable.',              '1d6',  'Perforant',  'Ranged', 1.0, 25.0,  '[""À deux mains"",""Portée (24/96m)""]'),
    ('Arc long',     'Weapon', 'Arc puissant à longue portée.',     '1d8',  'Perforant',  'Ranged', 2.0, 50.0,  '[""Lourde"",""À deux mains"",""Portée (45/180m)""]'),
    ('Arbalète légère', 'Weapon', 'Arbalète maniable.',             '1d8',  'Perforant',  'Ranged', 5.0, 25.0,  '[""Portée (24/96m)"",""Chargement"",""À deux mains""]'),
    ('Arbalète lourde', 'Weapon', 'Arbalète puissante et lourde.', '1d10', 'Perforant',  'Ranged', 18.0,50.0,  '[""Lourde"",""Portée (30/120m)"",""Chargement"",""À deux mains""]'),
    ('Fléau',        'Weapon', 'Arme contondante à chaîne.',        '1d8',  'Contondant', 'Melee',  2.0, 10.0,  '[]'),
    ('Faux de guerre', 'Weapon', 'Grande faux à deux mains.',       '2d4',  'Tranchant',  'Melee',  6.0, 30.0,  '[""Lourde"",""Portée étendue"",""À deux mains""]'),
    ('Javeline',     'Weapon', 'Javelot simple lançable.',          '1d6',  'Perforant',  'Melee',  2.0, 0.5,   '[""Lançable (portée 9/36m)""]');

    -- Armors
    INSERT INTO [dbo].[DndItems] ([Name],[Category],[Description],[ArmorClassBonus],[Weight],[CostGp]) VALUES
    ('Armure matelassée', 'Armor', 'Armure légère en tissu rembourré.', 11, 8.0, 5.0),
    ('Armure de cuir',    'Armor', 'Armure légère en cuir endurci.',    11, 10.0, 10.0),
    ('Armure de cuir clouté', 'Armor', 'Cuir renforcé de rivets métalliques.', 12, 13.0, 45.0),
    ('Chemise de mailles', 'Armor', 'Armure intermédiaire en anneaux.', 13, 20.0, 50.0),
    ('Armure d''écailles', 'Armor', 'Armure intermédiaire en écailles.', 14, 45.0, 50.0),
    ('Cuirasse',       'Armor', 'Plastron métallique solide.',           14, 20.0, 400.0),
    ('Demi-plate',     'Armor', 'Armure intermédiaire complète.',        15, 20.0, 750.0),
    ('Armure d''anneaux', 'Armor', 'Armure légère en anneaux.',         14, 40.0, 30.0),
    ('Cotte de mailles', 'Armor', 'Armure lourde en anneaux entrelacés.',16, 55.0, 75.0),
    ('Armure à bandes', 'Armor', 'Armure lourde en bandes superposées.',17, 60.0, 200.0),
    ('Armure complète', 'Armor', 'Armure lourde couvrant tout le corps.',18, 65.0, 1500.0),
    ('Bouclier',       'Armor', 'Bouclier en bois ou métal (+2 CA).',    2, 6.0, 10.0);

    -- Potions
    INSERT INTO [dbo].[DndItems] ([Name],[Category],[Description],[HealingDice],[Weight],[CostGp]) VALUES
    ('Potion de soins',        'Potion', 'Restaure 2d4+2 points de vie.',   '2d4+2', 0.5, 50.0),
    ('Potion de soins supérieure', 'Potion', 'Restaure 4d4+4 points de vie.', '4d4+4', 0.5, 100.0),
    ('Potion de soins excellente', 'Potion', 'Restaure 8d4+8 points de vie.', '8d4+8', 0.5, 500.0),
    ('Potion de soins suprême',    'Potion', 'Restaure 10d4+20 points de vie.', '10d4+20', 0.5, 1350.0);
END");

        // ── Spells ────────────────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndSpells] WHERE [Name] = 'Boule de feu')
BEGIN
    INSERT INTO [dbo].[DndSpells] ([Name],[Level],[School],[Description],[CastingTime],[Range],[Duration],[DamageDice],[DamageType],[Classes],[Components],[RequiresConcentration],[IsRitual]) VALUES
    -- Cantrips
    ('Prestidigitation', 0, 'Transmutation', 'Créez de petits effets magiques mineurs.', '1 action', '3 mètres', '1 heure', NULL, NULL, '[""Wizard"",""Sorcerer"",""Bard"",""Warlock""]', 'VS', 0, 0),
    ('Bouffée de poison', 0, 'Conjuration', 'Projetez un cône de gaz empoisonné sur une créature.', '1 action', '3 mètres', 'Instantané', '1d12', 'Poison', '[""Druid"",""Warlock"",""Wizard""]', 'VS', 0, 0),
    ('Lance de feu', 0, 'Évocation', 'Lance un trait de feu sur une créature.', '1 action', '36 mètres', 'Instantané', '1d10', 'Feu', '[""Druid"",""Sorcerer"",""Wizard""]', 'VS', 0, 0),
    ('Rayon de givre', 0, 'Évocation', 'Rayon de froid glacial qui ralentit la cible.', '1 action', '18 mètres', 'Instantané', '1d8', 'Froid', '[""Sorcerer"",""Wizard""]', 'VS', 0, 0),
    ('Contact glacial', 0, 'Nécromancie', 'Main fantomatique qui entrave la régénération.', '1 action', '18 mètres', '1 round', '1d8', 'Nécrotique', '[""Warlock"",""Wizard"",""Sorcerer""]', 'VS', 0, 0),
    ('Lumières dansantes', 0, 'Évocation', 'Créez jusqu''à quatre lumières qui se déplacent.', '1 action', '36 mètres', 'Concentration, 1 minute', NULL, NULL, '[""Bard"",""Sorcerer"",""Wizard""]', 'VSM', 1, 0),
    ('Mot de radiance', 0, 'Évocation', 'Radiance brûlante autour de vous.', '1 action', 'Soi (rayon 1,5m)', 'Instantané', '1d6', 'Radiant', '[""Cleric""]', 'V', 0, 0),
    ('Flamme sacrée', 0, 'Évocation', 'Flamme radieuse qui tombe sur une créature.', '1 action', '18 mètres', 'Instantané', '1d8', 'Radiant', '[""Cleric""]', 'VS', 0, 0),
    ('Épargne des mourants', 0, 'Nécromancie', 'Stabilise une créature à 0 PV.', '1 action', 'Tactile', 'Instantané', NULL, NULL, '[""Cleric"",""Druid""]', 'VS', 0, 0),
    ('Coup de tonnerre', 0, 'Évocation', 'Onde de force sonique dans une zone.', '1 action', 'Soi (cube 1,5m)', 'Instantané', '1d6', 'Tonnerre', '[""Bard"",""Druid"",""Sorcerer"",""Wizard""]', 'VS', 0, 0),

    -- Niveau 1
    ('Projectile magique', 1, 'Évocation', 'Trois dards de force magique. Touche automatiquement.', '1 action', '36 mètres', 'Instantané', '1d4+1', 'Force', '[""Wizard"",""Sorcerer""]', 'VS', 0, 0),
    ('Soins', 1, 'Évocation', 'Restaure 1d8+mod PV à une créature touchée.', '1 action', 'Tactile', 'Instantané', '1d8', 'Soin', '[""Cleric"",""Druid"",""Bard"",""Paladin"",""Ranger""]', 'VS', 0, 0),
    ('Armure de mage', 1, 'Abjuration', 'CA devient 13 + mod Dex pour créature sans armure.', '1 action', 'Tactile', '8 heures', NULL, NULL, '[""Sorcerer"",""Wizard""]', 'VSM', 0, 0),
    ('Bouclier', 1, 'Abjuration', '+5 CA jusqu''au début de votre prochain tour (réaction).', '1 réaction', 'Soi', '1 round', NULL, NULL, '[""Sorcerer"",""Wizard""]', 'VS', 0, 0),
    ('Charme-personne', 1, 'Enchantement', 'Une créature humanoïde vous considère comme un ami.', '1 action', '9 mètres', '1 heure', NULL, NULL, '[""Bard"",""Druid"",""Sorcerer"",""Warlock"",""Wizard""]', 'VS', 0, 0),
    ('Sommeil', 1, 'Enchantement', 'Endort les créatures avec le moins de PV d''abord.', '1 action', '27 mètres', '1 minute', NULL, NULL, '[""Bard"",""Sorcerer"",""Wizard""]', 'VSM', 0, 0),
    ('Frayeur', 1, 'Illusion', 'Créez des illusions effrayantes pour terrifier des créatures.', '1 action', '24 mètres', 'Concentration, 1 minute', NULL, NULL, '[""Bard"",""Sorcerer"",""Warlock"",""Wizard""]', 'VSM', 1, 0),
    ('Graisse', 1, 'Conjuration', 'Zone de graisse glissante recouvre le sol.', '1 action', '18 mètres', 'Concentration, 1 minute', NULL, NULL, '[""Wizard""]', 'VSM', 1, 0),
    ('Marque du chasseur', 1, 'Divination', 'Marquez une créature pour lui infliger 1d6 dégâts supplémentaires.', '1 action bonus', '27 mètres', 'Concentration, 1 heure', '1d6', 'Aucun', '[""Ranger""]', 'V', 1, 0),
    ('Bénédiction', 1, 'Enchantement', 'Jusqu''à 3 créatures gagnent 1d4 aux jets d''attaque et de sauvegarde.', '1 action', '9 mètres', 'Concentration, 1 minute', NULL, NULL, '[""Cleric"",""Paladin""]', 'VSM', 1, 0),
    ('Frappe tonnante', 1, 'Évocation', 'Attaque + 2d6 tonnerre au premier toucher.', '1 action bonus', 'Soi', 'Concentration, 1 minute', '2d6', 'Tonnerre', '[""Paladin""]', 'V', 1, 0),
    ('Injonction', 1, 'Enchantement', 'Ordonnez à une créature d''exécuter une action simple.', '1 action', '18 mètres', '1 round', NULL, NULL, '[""Cleric"",""Paladin""]', 'V', 0, 0),
    ('Détection de la magie', 1, 'Divination', 'Percevez la présence de magie à 9 mètres.', '1 action', 'Soi', 'Concentration, 10 minutes', NULL, NULL, '[""Bard"",""Cleric"",""Druid"",""Paladin"",""Ranger"",""Sorcerer"",""Wizard""]', 'VS', 1, 1),

    -- Niveau 2
    ('Boule de feu', 3, 'Évocation', 'Explosion incendiaire de 12m de rayon, 8d6 dégâts de feu.', '1 action', '45 mètres', 'Instantané', '8d6', 'Feu', '[""Wizard"",""Sorcerer""]', 'VSM', 0, 0),
    ('Éclair', 3, 'Évocation', 'Décharge de 30m, 8d6 dégâts de foudre.', '1 action', 'Soi (ligne 30m)', 'Instantané', '8d6', 'Foudre', '[""Wizard"",""Sorcerer""]', 'VSM', 0, 0),
    ('Boule de foudre', 2, 'Évocation', 'Sphère de foudre qui rebondit dans une zone.', '1 action', '18 mètres', 'Concentration, 1 minute', '2d6', 'Foudre', '[""Sorcerer"",""Wizard""]', 'VSM', 1, 0),
    ('Ténèbres', 2, 'Évocation', 'Obscurité magique dans une sphère de 4,5m.', '1 action', '18 mètres', 'Concentration, 10 minutes', NULL, NULL, '[""Sorcerer"",""Warlock"",""Wizard""]', 'VM', 1, 0),
    ('Invisibilité', 2, 'Illusion', 'Une créature devient invisible jusqu''à l''attaque ou sort.', '1 action', 'Tactile', 'Concentration, 1 heure', NULL, NULL, '[""Bard"",""Sorcerer"",""Warlock"",""Wizard""]', 'VSM', 1, 0),
    ('Fracassement', 2, 'Évocation', 'Forte vibration sonore dans une sphère de 3m.', '1 action', '18 mètres', 'Instantané', '3d8', 'Tonnerre', '[""Bard"",""Sorcerer"",""Warlock"",""Wizard""]', 'VSM', 0, 0),
    ('Toile d''araignée', 2, 'Conjuration', 'Zone de toiles qui retient les créatures.', '1 action', '18 mètres', 'Concentration, 1 heure', NULL, NULL, '[""Druid"",""Sorcerer"",""Wizard""]', 'VSM', 1, 0),
    ('Soins en masse', 5, 'Évocation', 'Restaure jusqu''à 6 créatures de 3d8+mod PV.', '1 action', '18 mètres', 'Instantané', '3d8', 'Soin', '[""Cleric"",""Druid""]', 'VS', 0, 0),

    -- Niveau 4-5
    ('Boule de feu améliorée', 5, 'Évocation', 'Version améliorée de la boule de feu.', '1 action', '45 mètres', 'Instantané', '10d6', 'Feu', '[""Wizard"",""Sorcerer""]', 'VSM', 0, 0),
    ('Téléporation', 7, 'Conjuration', 'Transportez jusqu''à 8 créatures sur de longues distances.', '1 action', '3 mètres', 'Instantané', NULL, NULL, '[""Bard"",""Sorcerer"",""Wizard""]', 'V', 0, 0),
    ('Météores enflammés', 8, 'Évocation', 'Pluie de météores enflammés sur une zone.', '1 action', '180 mètres', 'Instantané', '20d6', 'Feu', '[""Cleric"",""Druid"",""Sorcerer"",""Wizard""]', 'VSM', 0, 0),
    ('Souhait', 9, 'Conjuration', 'Sort le plus puissant du jeu. Réalise un vœu.', '1 action', 'Soi', 'Instantané', NULL, NULL, '[""Sorcerer"",""Wizard""]', 'V', 0, 0),
    ('Nuée de météorites', 9, 'Évocation', 'Boules de feu et de glace sur 4 zones de 12m.', '1 action', '300 mètres', 'Instantané', '20d6', 'Feu', '[""Druid"",""Sorcerer"",""Wizard""]', 'VSM', 0, 0);
END");

        // ── Monster Templates ─────────────────────────────────────────────────
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[DndMonsterTemplates] WHERE [Name] = 'Gobelin')
BEGIN
    INSERT INTO [dbo].[DndMonsterTemplates] ([Name],[MonsterType],[ChallengeRating],[Description],[HitPoints],[HitDice],[ArmorClass],[Speed],[Strength],[Dexterity],[Constitution],[Intelligence],[Wisdom],[Charisma],[Actions],[SpecialAbilities],[Alignment]) VALUES
    ('Gobelin', 'Humanoïde', '1/4', 'Petite créature malveillante vivant en groupes.', 7, '2d6', 15, 30, 8, 14, 10, 10, 8, 8,
     '[{""name"":""Cimeterre"",""attackBonus"":4,""damageDice"":""1d6+2"",""damageType"":""Tranchant""},{""name"":""Arc court"",""attackBonus"":4,""damageDice"":""1d6+2"",""damageType"":""Perforant"",""range"":""24/96m""}]',
     '[""Fuite nimble"",""Cœur de l ombre""]', 'Neutre Mauvais'),

    ('Orque', 'Humanoïde', '1/2', 'Guerrier brutal et agressif.', 15, '2d8+6', 13, 30, 16, 12, 16, 7, 11, 10,
     '[{""name"":""Hache à deux mains"",""attackBonus"":5,""damageDice"":""1d12+3"",""damageType"":""Tranchant""},{""name"":""Javeline"",""attackBonus"":5,""damageDice"":""1d6+3"",""damageType"":""Perforant"",""range"":""9/36m""}]',
     '[""Agressivité""]', 'Chaotique Mauvais'),

    ('Squelette', 'Mort-vivant', '1/4', 'Ossements animés par de la nécromancie.', 13, '2d8+4', 13, 30, 10, 14, 15, 6, 8, 5,
     '[{""name"":""Épée courte"",""attackBonus"":4,""damageDice"":""1d6+2"",""damageType"":""Perforant""},{""name"":""Arc court"",""attackBonus"":4,""damageDice"":""1d6+2"",""damageType"":""Perforant"",""range"":""24/96m""}]',
     '[""Immunité poison"",""Immunité épuisement""]', 'Loyal Mauvais'),

    ('Zombie', 'Mort-vivant', '1/4', 'Cadavre animé par de la nécromancie sombre.', 22, '3d8+9', 8, 20, 13, 6, 16, 3, 6, 5,
     '[{""name"":""Coup"",""attackBonus"":3,""damageDice"":""1d6+1"",""damageType"":""Contondant""}]',
     '[""Ténacité des morts-vivants"",""Immunité poison""]', 'Neutre Mauvais'),

    ('Loup', 'Bête', '1/4', 'Prédateur chassant en meute.', 11, '2d8+2', 13, 40, 12, 15, 12, 3, 12, 6,
     '[{""name"":""Morsure"",""attackBonus"":4,""damageDice"":""2d4+2"",""damageType"":""Perforant""}]',
     '[""Tactique de meute"",""Odorat et ouïe aiguisés""]', 'Neutre'),

    ('Bandit', 'Humanoïde', '1/8', 'Criminel de grand chemin armé.', 11, '2d8+2', 12, 30, 11, 12, 12, 10, 10, 10,
     '[{""name"":""Cimeterre"",""attackBonus"":3,""damageDice"":""1d6+1"",""damageType"":""Tranchant""},{""name"":""Arc léger"",""attackBonus"":3,""damageDice"":""1d8+1"",""damageType"":""Perforant"",""range"":""24/96m""}]',
     '[]', 'Neutre Mauvais'),

    ('Chevalier', 'Humanoïde', '3', 'Guerrier lourdement armé au service d un noble.', 52, '8d8+16', 18, 30, 16, 11, 14, 11, 11, 15,
     '[{""name"":""Épée longue"",""attackBonus"":5,""damageDice"":""1d8+3"",""damageType"":""Tranchant""}]',
     '[""Commande (recharge 5-6)"",""Sauvetage courageux""]', 'Loyal Bon'),

    ('Dragon rouge (jeune)', 'Dragon', '10', 'Jeune dragon rouge orgueilleux et territorial.', 178, '17d10+85', 18, 40, 23, 10, 21, 14, 11, 19,
     '[{""name"":""Griffe"",""attackBonus"":10,""damageDice"":""2d6+6"",""damageType"":""Tranchant""},{""name"":""Morsure"",""attackBonus"":10,""damageDice"":""2d10+6"",""damageType"":""Perforant""},{""name"":""Souffle de feu (recharge 5-6)"",""attackBonus"":0,""damageDice"":""16d6"",""damageType"":""Feu""}]',
     '[""Immunité au feu"",""Vol 40m"",""Résistance draconique""]', 'Chaotique Mauvais'),

    ('Dragon rouge (adulte)', 'Dragon', '17', 'Dragon rouge adulte d une puissance terrifiante.', 256, '19d12+114', 19, 40, 27, 10, 25, 16, 13, 21,
     '[{""name"":""Griffe"",""attackBonus"":13,""damageDice"":""2d6+8"",""damageType"":""Tranchant""},{""name"":""Morsure"",""attackBonus"":13,""damageDice"":""2d10+8"",""damageType"":""Perforant""},{""name"":""Souffle de feu (recharge 5-6)"",""attackBonus"":0,""damageDice"":""18d6"",""damageType"":""Feu""},{""name"":""Queue"",""attackBonus"":13,""damageDice"":""2d8+8"",""damageType"":""Contondant""}]',
     '[""Immunité au feu"",""Vol 40m"",""Présence terrifiante"",""Actions légendaires""]', 'Chaotique Mauvais'),

    ('Troll', 'Géant', '5', 'Brute régénérante à l odorat aiguisé.', 84, '8d10+40', 15, 30, 18, 13, 20, 7, 9, 7,
     '[{""name"":""Griffes"",""attackBonus"":7,""damageDice"":""2d6+4"",""damageType"":""Tranchant""},{""name"":""Morsure"",""attackBonus"":7,""damageDice"":""1d6+4"",""damageType"":""Perforant""}]',
     '[""Régénération"",""Odorat aiguisé""]', 'Chaotique Mauvais'),

    ('Vampire', 'Mort-vivant', '13', 'Noble vampire ancien aux pouvoirs redoutables.', 144, '17d8+68', 16, 30, 20, 18, 18, 17, 15, 18,
     '[{""name"":""Griffe"",""attackBonus"":9,""damageDice"":""2d8+5"",""damageType"":""Tranchant""},{""name"":""Morsure"",""attackBonus"":9,""damageDice"":""1d6+5"",""damageType"":""Perforant, drain""}]',
     '[""Régénération"",""Résistances multiples"",""Envoûtement"",""Enfants de la nuit"",""Changement de forme"",""Forme de brume"",""Pied de brume"",""Ascension des araignées""]', 'Loyal Mauvais'),

    ('Beholder', 'Aberration', '13', 'Sphère flottante aux multiples yeux magiques.', 180, '19d10+76', 18, 0, 10, 14, 18, 17, 15, 17,
     '[{""name"":""Morsure"",""attackBonus"":5,""damageDice"":""4d6"",""damageType"":""Perforant""},{""name"":""Rayon d oeil (×3/round)"",""attackBonus"":7,""damageDice"":""Variable"",""damageType"":""Variable""}]',
     '[""Résistance antimagie (central)"",""Rayons d yeux (10 types)"",""Vol""]', 'Loyal Mauvais'),

    ('Limon gélatineux', 'Vase', '2', 'Blob translucide qui dévore tout sur son passage.', 84, '8d10+40', 6, 15, 14, 3, 20, 1, 6, 1,
     '[{""name"":""Pseudopode"",""attackBonus"":4,""damageDice"":""3d6+2"",""damageType"":""Acide""}]',
     '[""Transparent"",""Amiboïde"",""Engloutissement""]', 'Neutre'),

    ('Minotaure', 'Monstreux', '3', 'Bête à tête de taureau vivant dans des labyrinthes.', 76, '9d10+27', 14, 40, 18, 11, 16, 6, 16, 9,
     '[{""name"":""Hache à deux mains"",""attackBonus"":6,""damageDice"":""2d12+4"",""damageType"":""Tranchant""},{""name"":""Coup de corne"",""attackBonus"":6,""damageDice"":""2d8+4"",""damageType"":""Perforant""}]',
     '[""Charge"",""Labyrinthe parfait""]', 'Chaotique Mauvais'),

    ('Géant des collines', 'Géant', '5', 'Géant stupide et brutal.', 105, '10d12+40', 13, 40, 21, 8, 19, 5, 9, 6,
     '[{""name"":""Massue"",""attackBonus"":8,""damageDice"":""3d8+5"",""damageType"":""Contondant""},{""name"":""Pierre lancée"",""attackBonus"":8,""damageDice"":""3d10+5"",""damageType"":""Contondant"",""range"":""18/72m""}]',
     '[""Grosse pierre""]', 'Chaotique Mauvais'),

    ('Vampire (valet)', 'Mort-vivant', '5', 'Serviteur vampirique à la puissance réduite.', 82, '11d8+33', 16, 30, 16, 16, 16, 11, 10, 12,
     '[{""name"":""Griffe"",""attackBonus"":6,""damageDice"":""2d6+3"",""damageType"":""Tranchant""},{""name"":""Morsure"",""attackBonus"":6,""damageDice"":""1d6+3"",""damageType"":""Perforant, drain""}]',
     '[""Régénération"",""Résistances"",""Escalade des araignées""]', 'Neutre Mauvais'),

    ('Elementaire de feu', 'Elementaire', '5', 'Manifestation vivante de la flamme.', 102, '12d10+36', 13, 15, 10, 17, 16, 6, 10, 7,
     '[{""name"":""Toucher"",""attackBonus"":6,""damageDice"":""2d6+3"",""damageType"":""Feu""}]',
     '[""Forme de feu"",""Illumination"",""Immunité au feu"",""Vulnérabilité au froid""]', 'Neutre'),

    ('Fantôme', 'Mort-vivant', '4', 'Ame tourmentée liée au monde des vivants.', 45, '10d8', 11, 0, 7, 13, 10, 10, 12, 11,
     '[{""name"":""Toucher affaiblissant"",""attackBonus"":5,""damageDice"":""4d6"",""damageType"":""Nécrotique""},{""name"":""Possession"",""attackBonus"":0,""damageDice"":""0"",""damageType"":""Contrôle""}]',
     '[""Incorporel"",""Résistances multiples"",""Vol"",""Regard horrible""]', 'Neutre'),

    ('Araignée géante', 'Bête', '1', 'Araignée de grande taille chassant avec des toiles.', 26, '4d10+4', 14, 30, 14, 16, 12, 2, 11, 4,
     '[{""name"":""Morsure"",""attackBonus"":5,""damageDice"":""1d8+3"",""damageType"":""Perforant""}]',
     '[""Perception des toiles"",""Marche sur les toiles"",""Escalade""]', 'Neutre'),

    ('Hydre', 'Monstreux', '8', 'Reptile à têtes multiples régénérantes.', 172, '15d12+60', 15, 30, 20, 12, 20, 2, 10, 7,
     '[{""name"":""Morsures (×5)"",""attackBonus"":8,""damageDice"":""1d10+5"",""damageType"":""Perforant""}]',
     '[""Têtes multiples"",""Réactivité"",""Régénération de têtes""]', 'Neutre');
END");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[DndMonsterTemplates]', N'U') IS NOT NULL DROP TABLE [dbo].[DndMonsterTemplates]");
        migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[DndSpells]', N'U') IS NOT NULL DROP TABLE [dbo].[DndSpells]");
        migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[DndItems]', N'U') IS NOT NULL DROP TABLE [dbo].[DndItems]");
        migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[DndClasses]', N'U') IS NOT NULL DROP TABLE [dbo].[DndClasses]");
        migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[DndRaces]', N'U') IS NOT NULL DROP TABLE [dbo].[DndRaces]");
        migrationBuilder.Sql("IF COL_LENGTH('[dbo].[NonPlayerCharacters]', 'GameSpecificData') IS NOT NULL ALTER TABLE [dbo].[NonPlayerCharacters] DROP COLUMN [GameSpecificData]");
        migrationBuilder.Sql("IF COL_LENGTH('[dbo].[Characters]', 'SourceCharacterId') IS NOT NULL BEGIN ALTER TABLE [dbo].[Characters] DROP CONSTRAINT IF EXISTS [FK_Characters_Characters_SourceCharacterId]; ALTER TABLE [dbo].[Characters] DROP COLUMN [SourceCharacterId]; END");
        migrationBuilder.Sql("IF COL_LENGTH('[dbo].[Characters]', 'IsBaseCharacter') IS NOT NULL ALTER TABLE [dbo].[Characters] DROP COLUMN [IsBaseCharacter]");
    }
}
