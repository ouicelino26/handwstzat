-- Generated from wwwroot/images/player-photos/lbe/lbe-player-photos-db-matches.json
-- Synchronizes safe LFH/LBE portrait matches into players.Photo.
-- Matched players: 225
-- Unmatched DB players left untouched: 115

START TRANSACTION;

SET @column_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'players'
      AND COLUMN_NAME = 'Photo'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `players` ADD COLUMN `Photo` varchar(500) NULL',
    'SELECT ''players.Photo already exists'' AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- PELLERIN MANON / TOULON <- Manon PELLERIN
UPDATE `players` SET `Photo` = 'pellerin-manon-822.png' WHERE `Id` = 3;
-- BERRAIS LOLA / PLAN-DE-CUQUES <- Lola BERRAIS
UPDATE `players` SET `Photo` = 'berrais-lola-14508.png' WHERE `Id` = 4;
-- FINSTAD BERGUM MARI / TOULON <- Mari Finstad BERGUM
UPDATE `players` SET `Photo` = 'bergum-mari-finstad-1132.png' WHERE `Id` = 7;
-- LOQUAY MANON / PLAN-DE-CUQUES <- Manon LOQUAY
UPDATE `players` SET `Photo` = 'loquay-manon-757.png' WHERE `Id` = 9;
-- GASSIOLLE MANEA / TOULON <- Manéa GASSIOLLE
UPDATE `players` SET `Photo` = 'gassiolle-manea-26999.png' WHERE `Id` = 10;
-- RIVAL CLEMENTINE / TOULON <- Clémentine RIVAL
UPDATE `players` SET `Photo` = 'rival-clementine-14282.png' WHERE `Id` = 11;
-- FOLITUU NORAH / TOULON <- Norah FOLITUU
UPDATE `players` SET `Photo` = 'folituu-norah-14148.png' WHERE `Id` = 13;
-- THEVENET JADE / TOULON <- JADE THEVENET
UPDATE `players` SET `Photo` = 'thevenet-jade-27001.png' WHERE `Id` = 14;
-- ZIMMERMAN KRISTY / TOULON <- Kristy ZIMMERMAN
UPDATE `players` SET `Photo` = 'zimmerman-kristy-14113.png' WHERE `Id` = 17;
-- SAMZUN ZAZIE / TOULON <- Zazie SAMZUN
UPDATE `players` SET `Photo` = 'samzun-zazie-14063.png' WHERE `Id` = 18;
-- SOUSA BEATRIZ / TOULON <- Beatriz SOUSA
UPDATE `players` SET `Photo` = 'sousa-beatriz-14338.png' WHERE `Id` = 20;
-- KANOR LAURA / BREST <- Laura KANOR
UPDATE `players` SET `Photo` = 'kanor-laura-26545.png' WHERE `Id` = 21;
-- LOTT ANNIKA / BREST <- Annika LOTT
UPDATE `players` SET `Photo` = 'lott-annika-26546.png' WHERE `Id` = 22;
-- LASSOURCE CORALIE / BREST <- Coralie LASSOURCE
UPDATE `players` SET `Photo` = 'lassource-coralie-22671.png' WHERE `Id` = 23;
-- FAURE JULIETTE / BREST <- Juliette FAURE
UPDATE `players` SET `Photo` = 'faure-juliette-702.png' WHERE `Id` = 24;
-- NOCANDY MELINE / BREST <- Méline NOCANDY
UPDATE `players` SET `Photo` = 'nocandy-meline-744.png' WHERE `Id` = 25;
-- FOPPA PAULETTA / BREST <- Pauletta FOPPA
UPDATE `players` SET `Photo` = 'foppa-pauletta-829.png' WHERE `Id` = 26;
-- DELAYE SIOBANN / BREST <- Siobann DELAYE
UPDATE `players` SET `Photo` = 'delaye-siobann-1272.png' WHERE `Id` = 28;
-- MAIROT CLARISSE / BREST <- Clarisse MAIROT
UPDATE `players` SET `Photo` = 'mairot-clarisse-13725.png' WHERE `Id` = 29;
-- JARRIGE EVA / CHAMBRAY <- Eva JARRIGE
UPDATE `players` SET `Photo` = 'jarrige-eva-707.png' WHERE `Id` = 30;
-- ONDONO ORIANE / BREST <- Oriane ONDONO
UPDATE `players` SET `Photo` = 'ondono-oriane-657.png' WHERE `Id` = 31;
-- VIAKHIREVA ANNA / BREST <- Anna VYAKHIREVA
UPDATE `players` SET `Photo` = 'vyakhireva-anna-26543.png' WHERE `Id` = 32;
-- MLADENOVSKA IVA / BESANCON <- Iva MLADENOVSKA
UPDATE `players` SET `Photo` = 'mladenovska-iva-23297.png' WHERE `Id` = 33;
-- ANDRE FLORIANE / BREST <- Floriane ANDRE
UPDATE `players` SET `Photo` = 'andre-floriane-871.png' WHERE `Id` = 35;
-- COATANEA PAULINE / BREST <- Pauline COATANEA
UPDATE `players` SET `Photo` = 'coatanea-pauline-509.png' WHERE `Id` = 37;
-- KIBUEY PHELLYS / SAMBRE <- Phellys KIBUEY
UPDATE `players` SET `Photo` = 'kibuey-phellys-14491.png' WHERE `Id` = 40;
-- BORG LYLOU / MERIGNAC <- Lylou BORG
UPDATE `players` SET `Photo` = 'borg-lylou-23256.png' WHERE `Id` = 41;
-- CHANTELLY MELISSA / ACHENHEIM <- Mélissa CHANTELLY
UPDATE `players` SET `Photo` = 'chantelly-melissa-23259.png' WHERE `Id` = 42;
-- BORG ENOLA / BREST <- Enola BORG
UPDATE `players` SET `Photo` = 'borg-enola-23274.png' WHERE `Id` = 43;
-- MELIQUE MATHILDE / SAMBRE <- Mathilde MELIQUE
UPDATE `players` SET `Photo` = 'melique-mathilde-14489.png' WHERE `Id` = 45;
-- COLINOT LINA / ACHENHEIM <- Lina COLINOT
UPDATE `players` SET `Photo` = 'colinot-lina-23262.png' WHERE `Id` = 50;
-- SISSOKO ASSA / MERIGNAC <- Assa SISSOKO
UPDATE `players` SET `Photo` = 'sissoko-assa-30047.png' WHERE `Id` = 52;
-- GRANDVEAU LENA / METZ <- Léna GRANDVEAU
UPDATE `players` SET `Photo` = 'grandveau-lena-14051.png' WHERE `Id` = 57;
-- VALENTINI CHLOE / METZ <- Chloé VALENTINI
UPDATE `players` SET `Photo` = 'valentini-chloe-513.png' WHERE `Id` = 58;
-- AUGUSTINE ANNE EMMANUELLE / METZ <- Anne-Emmanuelle AUGUSTINE
UPDATE `players` SET `Photo` = 'augustine-anne-emmanuelle-14062.png' WHERE `Id` = 61;
-- AXNER TYRA / METZ <- Tyra AXNER
UPDATE `players` SET `Photo` = 'axner-tyra-26567.png' WHERE `Id` = 62;
-- MLAMALI ZALIATA / BESANCON <- Zaliata MLAMALI
UPDATE `players` SET `Photo` = 'mlamali-zaliata-14025.png' WHERE `Id` = 63;
-- BOUKTIT SARAH / METZ <- Sarah BOUKTIT
UPDATE `players` SET `Photo` = 'bouktit-sarah-1220.png' WHERE `Id` = 65;
-- GRANIER LUCIE / METZ <- Lucie GRANIER
UPDATE `players` SET `Photo` = 'granier-lucie-701.png' WHERE `Id` = 66;
-- DEPUISET CAMILLE / METZ <- Camille DEPUISET
UPDATE `players` SET `Photo` = 'depuiset-camille-601.png' WHERE `Id` = 70;
-- GOLVET DELIA-MARIE-NELLY / PARIS92 <- Délia GOLVET
UPDATE `players` SET `Photo` = 'golvet-delia-14012.png' WHERE `Id` = 71;
-- VAMOS PETRA / METZ <- Petra VAMOS
UPDATE `players` SET `Photo` = 'vamos-petra-26570.png' WHERE `Id` = 73;
-- ADON SYRIANE / SAMBRE <- Syriane ADON
UPDATE `players` SET `Photo` = 'adon-syriane-23227.png' WHERE `Id` = 75;
-- BALLUREAU LEA / SAMBRE <- Léa BALLUREAU
UPDATE `players` SET `Photo` = 'ballureau-lea-14448.png' WHERE `Id` = 77;
-- CHALMANDRIER MAELLE / SAMBRE <- Maelle CHALMANDRIER
UPDATE `players` SET `Photo` = 'chalmandrier-maelle-959.png' WHERE `Id` = 78;
-- GODET INES / SAMBRE <- Ines GODET
UPDATE `players` SET `Photo` = 'godet-ines-14454.png' WHERE `Id` = 79;
-- KOUAYA MAELYS / SAMBRE <- Maëlys KOUAYA
UPDATE `players` SET `Photo` = 'kouaya-maelys-1022.png' WHERE `Id` = 81;
-- KVASOVA HANA / SAMBRE <- Hana KVASOVA
UPDATE `players` SET `Photo` = 'kvasova-hana-14443.png' WHERE `Id` = 82;
-- RUTIL KIMBERLEY / SAMBRE <- Kimberly RUTIL
UPDATE `players` SET `Photo` = 'rutil-kimberly-14470.png' WHERE `Id` = 83;
-- SIAS JULIE / SAMBRE <- Julie SIAS
UPDATE `players` SET `Photo` = 'sias-julie-14449.png' WHERE `Id` = 84;
-- TOURIGNY CAMILLE / SAMBRE <- Camille TOURIGNY
UPDATE `players` SET `Photo` = 'tourigny-camille-14469.png' WHERE `Id` = 85;
-- TOUBISSA-ELBECO JUSTICIA / SAMBRE <- Justicia TOUBISSA-ELBECO
UPDATE `players` SET `Photo` = 'toubissa-elbeco-justicia-979.png' WHERE `Id` = 86;
-- SY DIENABA / NICE OGC <- Dienaba SY
UPDATE `players` SET `Photo` = 'sy-dienaba-588.png' WHERE `Id` = 89;
-- BENEZETH LOUNA / NICE OGC <- Louna BENEZETH
UPDATE `players` SET `Photo` = 'benezeth-louna-26605.png' WHERE `Id` = 91;
-- HOLEJOVA ADRIANA / DIJON <- Adriana HOLEJOVA
UPDATE `players` SET `Photo` = 'holejova-adriana-14060.png' WHERE `Id` = 92;
-- DUPUIS MARINE / NICE OGC <- Marine DUPUIS
UPDATE `players` SET `Photo` = 'dupuis-marine-14050.png' WHERE `Id` = 93;
-- FALL MARIE / NICE OGC <- Marie FALL
UPDATE `players` SET `Photo` = 'fall-marie-762.png' WHERE `Id` = 94;
-- SAJKA MARIE HELENE / NICE OGC <- Marie-Hélène SAJKA
UPDATE `players` SET `Photo` = 'sajka-marie-helene-23380.png' WHERE `Id` = 96;
-- LE BLEVEC MARGOT / NICE OGC <- Margot LE BLEVEC
UPDATE `players` SET `Photo` = 'le-blevec-margot-1146.png' WHERE `Id` = 97;
-- ABDELMALEK EHSAN / NICE OGC <- Ehsan ABDELMALEK
UPDATE `players` SET `Photo` = 'abdelmalek-ehsan-26609.png' WHERE `Id` = 98;
-- COLIC MARIJA / NICE OGC <- Marija COLIC
UPDATE `players` SET `Photo` = 'colic-marija-542.png' WHERE `Id` = 99;
-- SEMEDO-MONTEIRO WENDY / NICE OGC <- Wendy SEMEDO-MONTEIRO
UPDATE `players` SET `Photo` = 'semedo-monteiro-wendy-1149.png' WHERE `Id` = 101;
-- BELLONNET CHLOE / BESANCON <- Chloé BELLONNET
UPDATE `players` SET `Photo` = 'bellonnet-chloe-16877.png' WHERE `Id` = 102;
-- DEMBELE SITHA LAUREEN / PLAN-DE-CUQUES <- Sitha Laureen DEMBELE
UPDATE `players` SET `Photo` = 'dembele-sitha-laureen-14061.png' WHERE `Id` = 105;
-- ANTONISSEN NELE / PLAN-DE-CUQUES <- Nele ANTONISSEN
UPDATE `players` SET `Photo` = 'antonissen-nele-1124.png' WHERE `Id` = 107;
-- KROMOSKA ROMANE / PLAN-DE-CUQUES <- Romane KROMOSKA
UPDATE `players` SET `Photo` = 'kromoska-romane-1241.png' WHERE `Id` = 109;
-- MARTEL JUSTINE / PLAN-DE-CUQUES <- Justine MARTEL
UPDATE `players` SET `Photo` = 'martel-justine-593.png' WHERE `Id` = 110;
-- MATHON HELENA / PLAN-DE-CUQUES <- Héléna MATHON
UPDATE `players` SET `Photo` = 'mathon-helena-14057.png' WHERE `Id` = 111;
-- FAUVARQUE LAURA / DIJON <- Laura FAUVARQUE
UPDATE `players` SET `Photo` = 'fauvarque-laura-27091.png' WHERE `Id` = 112;
-- NOVELLAN ANDREA / PLAN-DE-CUQUES <- Andréa NOVELLAN
UPDATE `players` SET `Photo` = 'novellan-andrea-14122.png' WHERE `Id` = 114;
-- JOBARD MELANIE / ST-AMAND <- Mélanie JOBARD
UPDATE `players` SET `Photo` = 'jobard-melanie-14106.png' WHERE `Id` = 119;
-- HRVATIN EMA / ST-AMAND <- Ema HRVATIN
UPDATE `players` SET `Photo` = 'hrvatin-ema-26665.png' WHERE `Id` = 121;
-- LE-BLEVEC JULIE / ST-AMAND <- Julie LE BLEVEC
UPDATE `players` SET `Photo` = 'le-blevec-julie-799.png' WHERE `Id` = 124;
-- FAYNEL MAELLE / CHAMBRAY <- Maëlle FAYNEL
UPDATE `players` SET `Photo` = 'faynel-maelle-824.png' WHERE `Id` = 125;
-- EBANGA PAOLA / ST-AMAND <- Paola EBANGA BABOGA
UPDATE `players` SET `Photo` = 'ebanga-baboga-paola-14114.png' WHERE `Id` = 126;
-- BOISORIEUX LOUISON / STELLA SAINT-MAUR <- Louison BOISORIEUX
UPDATE `players` SET `Photo` = 'boisorieux-louison-14073.png' WHERE `Id` = 128;
-- TONDS OPHELIE / ST-AMAND <- Ophélie TONDS
UPDATE `players` SET `Photo` = 'tonds-ophelie-746.png' WHERE `Id` = 133;
-- GUIRASSY MASSEITA / ST-AMAND <- Masseita GUIRASSY
UPDATE `players` SET `Photo` = 'guirassy-masseita-14079.png' WHERE `Id` = 134;
-- GENYAH SEPHORA / STELLA SAINT-MAUR <- Sephora GENYAH
UPDATE `players` SET `Photo` = 'genyah-sephora-26997.png' WHERE `Id` = 136;
-- PUGLIESE CHLOE / STELLA SAINT-MAUR <- Chloé PUGLIESE
UPDATE `players` SET `Photo` = 'pugliese-chloe-716.png' WHERE `Id` = 137;
-- KARAMOKO FATOU / STELLA SAINT-MAUR <- Fatou KARAMOKO
UPDATE `players` SET `Photo` = 'karamoko-fatou-23213.png' WHERE `Id` = 138;
-- MAURICE MEISSA / STELLA SAINT-MAUR <- Meissa MAURICE
UPDATE `players` SET `Photo` = 'maurice-meissa-15188.png' WHERE `Id` = 140;
-- PEILLON MELINA / STELLA SAINT-MAUR <- Mélina PEILLON
UPDATE `players` SET `Photo` = 'peillon-melina-980.png' WHERE `Id` = 141;
-- DELORME ELISE / STELLA SAINT-MAUR <- Elise DELORME
UPDATE `players` SET `Photo` = 'delorme-elise-1029.png' WHERE `Id` = 142;
-- PLOTTON PAULINE-MARIE / STELLA SAINT-MAUR <- Pauline PLOTTON
UPDATE `players` SET `Photo` = 'plotton-pauline-14476.png' WHERE `Id` = 143;
-- TOURE DJENEBA / STELLA SAINT-MAUR <- Djénéba TOURE
UPDATE `players` SET `Photo` = 'toure-djeneba-23211.png' WHERE `Id` = 144;
-- PASTOUR KYNCIA-LAURA / TOULON <- Kyncia PASTOUR
UPDATE `players` SET `Photo` = 'pastour-kyncia-14479.png' WHERE `Id` = 145;
-- REZGUI RAKIA / STELLA SAINT-MAUR <- Rakia REZGUI
UPDATE `players` SET `Photo` = 'rezgui-rakia-14465.png' WHERE `Id` = 146;
-- PIERRE ALEXIANE-BEATRICE / STELLA SAINT-MAUR <- Alexiane PIERRE
UPDATE `players` SET `Photo` = 'pierre-alexiane-23214.png' WHERE `Id` = 148;
-- LACHAT MARIE / STELLA SAINT-MAUR <- Marie LACHAT
UPDATE `players` SET `Photo` = 'lachat-marie-14453.png' WHERE `Id` = 149;
-- INJAI ZEINA / STELLA SAINT-MAUR <- Zeina Injai
UPDATE `players` SET `Photo` = 'injai-zeina-26981.png' WHERE `Id` = 151;
-- CISSOKHO AMINATA / PARIS92 <- Aminata CISSOKHO
UPDATE `players` SET `Photo` = 'cissokho-aminata-996.png' WHERE `Id` = 153;
-- KANOUTE COURA / PARIS92 <- Coura KANOUTE
UPDATE `players` SET `Photo` = 'kanoute-coura-14047.png' WHERE `Id` = 154;
-- TECHER ELISA / ST-AMAND <- Elisa TECHER
UPDATE `players` SET `Photo` = 'techer-elisa-14092.png' WHERE `Id` = 155;
-- MORETTO BARBARA / PARIS92 <- Barbara MORETTO
UPDATE `players` SET `Photo` = 'moretto-barbara-577.png' WHERE `Id` = 158;
-- THOBOR EMMANUELLE / PARIS92 <- Emmanuelle THOBOR
UPDATE `players` SET `Photo` = 'thobor-emmanuelle-14151.png' WHERE `Id` = 160;
-- PROUVENSIER MARIE / PARIS92 <- Marie PROUVENSIER
UPDATE `players` SET `Photo` = 'prouvensier-marie-579.png' WHERE `Id` = 161;
-- SERDAREVIC LEA / PARIS92 <- Léa SERDAREVIC
UPDATE `players` SET `Photo` = 'serdarevic-lea-524.png' WHERE `Id` = 164;
-- BLONBOU JANNELA / PARIS92 <- Jannela BLONBOU
UPDATE `players` SET `Photo` = 'blonbou-jannela-14048.png' WHERE `Id` = 165;
-- BEGON LILOU / ACHENHEIM <- Lilou BEGON
UPDATE `players` SET `Photo` = 'begon-lilou-14179.png' WHERE `Id` = 166;
-- AHLEN HANNA / NICE OGC <- Hanna AHLEN
UPDATE `players` SET `Photo` = 'ahlen-hanna-1178.png' WHERE `Id` = 167;
-- VLUG LISA / ACHENHEIM <- Lisa VLUG
UPDATE `players` SET `Photo` = 'vlug-lisa-23290.png' WHERE `Id` = 170;
-- MAURIN CANDICE / PARIS92 <- Candice MAURIN
UPDATE `players` SET `Photo` = 'maurin-candice-774.png' WHERE `Id` = 171;
-- ABDESSELAM DALILA / ACHENHEIM <- Dalila ABDESSELAM
UPDATE `players` SET `Photo` = 'abdesselam-dalila-14181.png' WHERE `Id` = 172;
-- IMHOF MARGAUX / ACHENHEIM <- Margaux IMHOF
UPDATE `players` SET `Photo` = 'imhof-margaux-14192.png' WHERE `Id` = 176;
-- GUERRIER CHARLENE / ACHENHEIM <- Charlène GUERRIER
UPDATE `players` SET `Photo` = 'guerrier-charlene-14131.png' WHERE `Id` = 178;
-- TUCCELLA EMMA / ACHENHEIM <- Emma TUCCELLA
UPDATE `players` SET `Photo` = 'tuccella-emma-14027.png' WHERE `Id` = 179;
-- TRONCIN FANY / ACHENHEIM <- Fany TRONCIN
UPDATE `players` SET `Photo` = 'troncin-fany-23285.png' WHERE `Id` = 181;
-- FARGUES LEA / ACHENHEIM <- Léa FARGUES
UPDATE `players` SET `Photo` = 'fargues-lea-14176.png' WHERE `Id` = 182;
-- LONBORG STINE / DIJON <- Stine NORKLIT LONBORG
UPDATE `players` SET `Photo` = 'norklit-lonborg-stine-14127.png' WHERE `Id` = 184;
-- VAUTIER CLAIRE / DIJON <- Claire VAUTIER
UPDATE `players` SET `Photo` = 'vautier-claire-884.png' WHERE `Id` = 185;
-- GAYET MAUREEN / DIJON <- Maureen GAYET
UPDATE `players` SET `Photo` = 'gayet-maureen-14133.png' WHERE `Id` = 186;
-- OFFENDAL NADIA / DIJON <- Nadia MIELKE-OFFENDAL
UPDATE `players` SET `Photo` = 'mielke-offendal-nadia-14016.png' WHERE `Id` = 188;
-- PLOTTON MATHILDE / PARIS92 <- Mathilde PLOTTON
UPDATE `players` SET `Photo` = 'plotton-mathilde-14475.png' WHERE `Id` = 189;
-- DURY NINA / DIJON <- Nina DURY
UPDATE `players` SET `Photo` = 'dury-nina-14129.png' WHERE `Id` = 190;
-- URBAN-MEDEL ROSARIO / DIJON <- Rosario URBAN MEDEL
UPDATE `players` SET `Photo` = 'urban-medel-rosario-1111.png' WHERE `Id` = 191;
-- PINTAT LILOU / DIJON <- Lilou PINTAT
UPDATE `players` SET `Photo` = 'pintat-lilou-14135.png' WHERE `Id` = 192;
-- VALERO SARAH / DIJON <- Sarah Valero Jodar
UPDATE `players` SET `Photo` = 'valero-jodar-sarah-1112.png' WHERE `Id` = 193;
-- PERRET NINA / PLAN-DE-CUQUES <- Nina PERRET
UPDATE `players` SET `Photo` = 'perret-nina-14137.png' WHERE `Id` = 194;
-- SIVERTSEN CELINE / DIJON <- Celine SIVERTSEN
UPDATE `players` SET `Photo` = 'sivertsen-celine-1033.png' WHERE `Id` = 195;
-- BLAISE DOROTHEE / DIJON <- Dorothée BLAISE
UPDATE `players` SET `Photo` = 'blaise-dorothee-23334.png' WHERE `Id` = 197;
-- DOS-REIS MANUELLA / DIJON <- Manuella DOS REIS
UPDATE `players` SET `Photo` = 'dos-reis-manuella-14123.png' WHERE `Id` = 198;
-- GIEGERICH ANN-CATHRIN / DIJON <- Ann-Cathrin GIEGERICH
UPDATE `players` SET `Photo` = 'giegerich-ann-cathrin-26557.png' WHERE `Id` = 199;
-- MAUNY CONSTANCE / CHAMBRAY <- Constance MAUNY
UPDATE `players` SET `Photo` = 'mauny-constance-828.png' WHERE `Id` = 201;
-- GRIMAUD MANON / CHAMBRAY <- Manon GRIMAUD
UPDATE `players` SET `Photo` = 'grimaud-manon-536.png' WHERE `Id` = 202;
-- MODENEL LUCIE / CHAMBRAY <- Lucie MODENEL
UPDATE `players` SET `Photo` = 'modenel-lucie-1085.png' WHERE `Id` = 203;
-- PULERI LAURIE / PLAN-DE-CUQUES <- Laurie PULERI
UPDATE `players` SET `Photo` = 'puleri-laurie-553.png' WHERE `Id` = 206;
-- ABDOU DAWIYA / CHAMBRAY <- Dawiya ABDOU
UPDATE `players` SET `Photo` = 'abdou-dawiya-23206.png' WHERE `Id` = 208;
-- DEBA MELVINE / CHAMBRAY <- Melvine DEBA
UPDATE `players` SET `Photo` = 'deba-melvine-14018.png' WHERE `Id` = 209;
-- MATTHIJS HOLMBERG VILMA / CHAMBRAY <- Vilma MATTHIJS HOLMBERG
UPDATE `players` SET `Photo` = 'matthijs-holmberg-vilma-26548.png' WHERE `Id` = 210;
-- MORVAN YAELLE / CHAMBRAY <- Yaelle MORVAN
UPDATE `players` SET `Photo` = 'morvan-yaelle-14156.png' WHERE `Id` = 211;
-- PERCHE EMMA / CHAMBRAY <- Emma PERCHE
UPDATE `players` SET `Photo` = 'perche-emma-1274.png' WHERE `Id` = 212;
-- STOILJKOVIC JOVANA / CHAMBRAY <- Jovana STOILJKOVIC
UPDATE `players` SET `Photo` = 'stoiljkovic-jovana-20729.png' WHERE `Id` = 214;
-- DI-ROCCO ILONA / BESANCON <- Ilona DI ROCCO
UPDATE `players` SET `Photo` = 'di-rocco-ilona-801.png' WHERE `Id` = 215;
-- WAJOKA SUZANNE / METZ <- Suzanne WAJOKA
UPDATE `players` SET `Photo` = 'wajoka-suzanne-978.png' WHERE `Id` = 216;
-- FRECON-DEMOUGE ALIZEE / BESANCON <- Alizée FRECON DEMOUGE
UPDATE `players` SET `Photo` = 'frecon-demouge-alizee-13724.png' WHERE `Id` = 217;
-- SOLSTAD CELINE / BESANCON <- Céline SOLSTAD
UPDATE `players` SET `Photo` = 'solstad-celine-26539.png' WHERE `Id` = 218;
-- LAMBET LALIE / BESANCON <- Lalie LAMBET
UPDATE `players` SET `Photo` = 'lambet-lalie-14462.png' WHERE `Id` = 220;
-- ROBERT PAULINE / BESANCON <- Pauline ROBERT
UPDATE `players` SET `Photo` = 'robert-pauline-625.png' WHERE `Id` = 221;
-- ZAZAI SABRINA / BESANCON <- Sabrina ZAZAÏ-ÖZIL
UPDATE `players` SET `Photo` = 'zazai-ozil-sabrina-512.png' WHERE `Id` = 223;
-- MANDRET CAMILLE / BESANCON <- Camille MANDRET
UPDATE `players` SET `Photo` = 'mandret-camille-26541.png' WHERE `Id` = 226;
-- TSHIMANGA KIARA / BREST <- Kiara TSHIMANGA
UPDATE `players` SET `Photo` = 'tshimanga-kiara-1235.png' WHERE `Id` = 227;
-- BELLEC EMILIE / BESANCON <- Emilie BELLEC
UPDATE `players` SET `Photo` = 'bellec-emilie-756.png' WHERE `Id` = 228;
-- MAIROT JULIETTE / BESANCON <- Juliette MAIROT
UPDATE `players` SET `Photo` = 'mairot-juliette-14382.png' WHERE `Id` = 230;
-- KINGUE PRUNELLE / BESANCON <- Prunelle KINGUE
UPDATE `players` SET `Photo` = 'kingue-prunelle-23319.png' WHERE `Id` = 231;
-- BONNET FLORENCE / BESANCON <- Florence BONNET
UPDATE `players` SET `Photo` = 'bonnet-florence-23683.png' WHERE `Id` = 232;
-- LERSTAD TONJE / BESANCON <- Tonje LERSTAD
UPDATE `players` SET `Photo` = 'lerstad-tonje-13730.png' WHERE `Id` = 233;
-- OLLIVIER ELISE / PARIS92 <- Elise OLLIVIER
UPDATE `players` SET `Photo` = 'ollivier-elise-28392.png' WHERE `Id` = 245;
-- ANDON JULILOVE / PARIS92 <- Julilove Andon
UPDATE `players` SET `Photo` = 'andon-julilove-29079.png' WHERE `Id` = 248;
-- ERRARD MANON / METZ <- Manon ERRARD
UPDATE `players` SET `Photo` = 'errard-manon-23333.png' WHERE `Id` = 256;
-- MUMBONGO CHARITE / BESANCON <- Charité MUMBONGO
UPDATE `players` SET `Photo` = 'mumbongo-charite-26540.png' WHERE `Id` = 260;
-- NIOMBLA GNONSIANE / DIJON <- Gnonsiane NIOMBLA
UPDATE `players` SET `Photo` = 'niombla-gnonsiane-28848.png' WHERE `Id` = 262;
-- DA CRUZ ISABELA / NICE OGC <- Isabela Da Cruz
UPDATE `players` SET `Photo` = 'da-cruz-isabela-29604.png' WHERE `Id` = 264;
-- LOUVEAU JULIETTE / DIJON <- Juliette LOUVEAU
UPDATE `players` SET `Photo` = 'louveau-juliette-14256.png' WHERE `Id` = 265;
-- NGONGANG INGRID / SAMBRE <- Ingrid NGONGANG
UPDATE `players` SET `Photo` = 'ngongang-ingrid-28027.png' WHERE `Id` = 266;
-- ALCEE LEANE / CHAMBRAY <- Leane Alcee
UPDATE `players` SET `Photo` = 'alcee-leane-29556.png' WHERE `Id` = 268;
-- AROTCARENA-ROSAS JONE / CHAMBRAY <- Jone Arotcarena-Rosas
UPDATE `players` SET `Photo` = 'arotcarena-rosas-jone-29555.png' WHERE `Id` = 269;
-- CAVANIE LOUISE / TOULON <- Louise Cavanié
UPDATE `players` SET `Photo` = 'cavanie-louise-28412.png' WHERE `Id` = 271;
-- KEROUANTON ADELE / NICE OGC <- Adèle Kerouanton
UPDATE `players` SET `Photo` = 'kerouanton-adele-29605.png' WHERE `Id` = 273;
-- STROMBERG CARIN / CHAMBRAY <- Carin STROMBERG
UPDATE `players` SET `Photo` = 'stromberg-carin-1129.png' WHERE `Id` = 274;
-- MOSTEFAOUI YOSR / ACHENHEIM <- Yosr Mostefaoui
UPDATE `players` SET `Photo` = 'mostefaoui-yosr-29636.png' WHERE `Id` = 278;
-- ATANGANA YVANA / METZ <- Yvana Atangana
UPDATE `players` SET `Photo` = 'atangana-yvana-29579.png' WHERE `Id` = 280;
-- DORELAS CASSY / STELLA SAINT-MAUR <- Cassy Dorelas
UPDATE `players` SET `Photo` = 'dorelas-cassy-26985.png' WHERE `Id` = 281;
-- HENRY LOUANE / DIJON <- Louane HENRY
UPDATE `players` SET `Photo` = 'henry-louane-27038.png' WHERE `Id` = 282;
-- N'DIAYE FILY / ACHENHEIM <- Fily N'Diaye
UPDATE `players` SET `Photo` = 'n-diaye-fily-1203.png' WHERE `Id` = 285;
-- ABDELLAHI SABRINA / ST-AMAND <- Sabrina ABDELLAHI
UPDATE `players` SET `Photo` = 'abdellahi-sabrina-14446.png' WHERE `Id` = 289;
-- MAUFFREY JULIA / ACHENHEIM <- Julia Mauffrey
UPDATE `players` SET `Photo` = 'mauffrey-julia-29633.png' WHERE `Id` = 290;
-- KOESTNER CLAIRE / ST-AMAND <- Claire KOESTNER
UPDATE `players` SET `Photo` = 'koestner-claire-23331.png' WHERE `Id` = 293;
-- ONDONO OPELIA / NICE OGC <- Opelia Ondono
UPDATE `players` SET `Photo` = 'ondono-opelia-29600.png' WHERE `Id` = 294;
-- SEMEDO-MONTEIRO WHITNEY / NICE OGC <- Whitney SEMEDO-MONTEIRO
UPDATE `players` SET `Photo` = 'semedo-monteiro-whitney-23463.png' WHERE `Id` = 295;
-- FOGGEA JULIE / PARIS92 <- JULIE FOGGEA
UPDATE `players` SET `Photo` = 'foggea-julie-21001.png' WHERE `Id` = 296;
-- BERGER MARIA / PARIS92 <- Maria Berger Wierzba
UPDATE `players` SET `Photo` = 'berger-wierzba-maria-29607.png' WHERE `Id` = 297;
-- BOUGUERCH KENZA / DIJON <- Kenza BOUGUERCHE
UPDATE `players` SET `Photo` = 'bouguerche-kenza-23402.png' WHERE `Id` = 302;
-- BENAM CHRISTIANA / PARIS92 <- Christiana Benam
UPDATE `players` SET `Photo` = 'benam-christiana-29612.png' WHERE `Id` = 303;
-- CANTIN MELINA / BREST <- Mélina CANTIN
UPDATE `players` SET `Photo` = 'cantin-melina-1243.png' WHERE `Id` = 304;
-- KARLSSON ELIN / ACHENHEIM <- Elin Karlsson
UPDATE `players` SET `Photo` = 'karlsson-elin-29629.png' WHERE `Id` = 305;
-- CUSSET LOUISE / ACHENHEIM <- Louise CUSSET
UPDATE `players` SET `Photo` = 'cusset-louise-815.png' WHERE `Id` = 306;
-- ARDOUIN NAEMI / SAMBRE <- Naémi ARDOUIN
UPDATE `players` SET `Photo` = 'ardouin-naemi-14053.png' WHERE `Id` = 307;
-- LOMBINDO LAURA-LYNE / SAMBRE <- Laura Lyne Lombindo
UPDATE `players` SET `Photo` = 'lombindo-laura-lyne-29615.png' WHERE `Id` = 308;
-- FRACHON ALBANE / ACHENHEIM <- Albane FRACHON
UPDATE `players` SET `Photo` = 'frachon-albane-14056.png' WHERE `Id` = 309;
-- SNEDKERUD NORA / ACHENHEIM <- Nora Snedkerud
UPDATE `players` SET `Photo` = 'snedkerud-nora-29630.png' WHERE `Id` = 310;
-- TIGNON ALIX / SAMBRE <- Alix TIGNON
UPDATE `players` SET `Photo` = 'tignon-alix-23279.png' WHERE `Id` = 311;
-- GONZALEZ LEANE / ACHENHEIM <- Léane Gonzalez
UPDATE `players` SET `Photo` = 'gonzalez-leane-29632.png' WHERE `Id` = 312;
-- STANKIEWICZ THEA / TOULON <- Théa Stankiewicz
UPDATE `players` SET `Photo` = 'stankiewicz-thea-29639.png' WHERE `Id` = 313;
-- GIUSTINIANI EVA / TOULON <- Eva GIUSTINIANI
UPDATE `players` SET `Photo` = 'giustiniani-eva-14285.png' WHERE `Id` = 314;
-- DEEN SOFIA / TOULON <- Sofia Deen
UPDATE `players` SET `Photo` = 'deen-sofia-29640.png' WHERE `Id` = 315;
-- DRAGENBERG JOSEFINE / TOULON <- Josefine Dragenberg
UPDATE `players` SET `Photo` = 'dragenberg-josefine-29641.png' WHERE `Id` = 316;
-- GROS ANA / BREST <- Ana Gros
UPDATE `players` SET `Photo` = 'gros-ana-29547.png' WHERE `Id` = 317;
-- CAYEZ MATHILDE / TOULON <- Mathilde CAYEZ
UPDATE `players` SET `Photo` = 'cayez-mathilde-23518.png' WHERE `Id` = 318;
-- MUNANOA JULIANNA / TOULON <- Julianna Munanoa
UPDATE `players` SET `Photo` = 'munanoa-julianna-29642.png' WHERE `Id` = 319;
-- DUIJNDAM RINKA / CHAMBRAY <- Rinka Duijndam
UPDATE `players` SET `Photo` = 'duijndam-rinka-29552.png' WHERE `Id` = 320;
-- SMITS XENIA / METZ <- Xenia Smits
UPDATE `players` SET `Photo` = 'smits-xenia-29575.png' WHERE `Id` = 321;
-- BUNDSEN JOHANNA / METZ <- Johanna BUNDSEN
UPDATE `players` SET `Photo` = 'bundsen-johanna-29742.png' WHERE `Id` = 322;
-- ALBEK ANNA / METZ <- Anna Albek
UPDATE `players` SET `Photo` = 'albek-anna-29573.png' WHERE `Id` = 323;
-- VAMA MARA / CHAMBRAY <- Mara Vama
UPDATE `players` SET `Photo` = 'vama-mara-29553.png' WHERE `Id` = 324;
-- NGOMBELE BETCHAIDELLE / METZ <- Betchaïdelle Ngombele
UPDATE `players` SET `Photo` = 'ngombele-betchaidelle-29576.png' WHERE `Id` = 325;
-- NOVOTNA SABRINA / METZ <- Sabrina Novotna
UPDATE `players` SET `Photo` = 'novotna-sabrina-29577.png' WHERE `Id` = 327;
-- GODARD LAURA / METZ <- Laura Godard
UPDATE `players` SET `Photo` = 'godard-laura-29572.png' WHERE `Id` = 328;
-- GROS BLANDINE / METZ <- Blandine GROS
UPDATE `players` SET `Photo` = 'gros-blandine-23269.png' WHERE `Id` = 329;
-- DJIOGAP VANESSA / HAVRE <- Vanessa DJIOGAP
UPDATE `players` SET `Photo` = 'djiogap-vanessa-23265.png' WHERE `Id` = 330;
-- DORP LAURA / HAVRE <- Laura DORP
UPDATE `players` SET `Photo` = 'dorp-laura-538.png' WHERE `Id` = 331;
-- BOUDEKHANE LORINE / HAVRE <- Lorine BOUDEKHANE
UPDATE `players` SET `Photo` = 'boudekhane-lorine-14054.png' WHERE `Id` = 332;
-- ROCHE LUCILE / HAVRE <- Lucile ROCHE
UPDATE `players` SET `Photo` = 'roche-lucile-14506.png' WHERE `Id` = 333;
-- AYONG LESLIE / HAVRE <- Leslie AYONG
UPDATE `players` SET `Photo` = 'ayong-leslie-14354.png' WHERE `Id` = 334;
-- RIBEIRO LOLA / HAVRE <- Lola RIBEIRO
UPDATE `players` SET `Photo` = 'ribeiro-lola-23212.png' WHERE `Id` = 335;
-- ERRIN CECILIA / NICE OGC <- Cecilia ERRIN
UPDATE `players` SET `Photo` = 'errin-cecilia-15190.png' WHERE `Id` = 336;
-- LE GARDIEN ZELIE / HAVRE <- Zélie LE GARDIEN
UPDATE `players` SET `Photo` = 'le-gardien-zelie-14343.png' WHERE `Id` = 337;
-- NKINDANDA CLEMENCE / HAVRE <- Clémence NKINDANDA
UPDATE `players` SET `Photo` = 'nkindanda-clemence-14021.png' WHERE `Id` = 338;
-- PIN VALENTINE / HAVRE <- Valentine PIN
UPDATE `players` SET `Photo` = 'pin-valentine-1270.png' WHERE `Id` = 339;
-- LEWANDOWSKI KAROLAIN / HAVRE <- Karolain Lewandowski
UPDATE `players` SET `Photo` = 'lewandowski-karolain-29568.png' WHERE `Id` = 340;
-- DIAWARA MATHITA / HAVRE <- Mathita DIAWARA
UPDATE `players` SET `Photo` = 'diawara-mathita-14066.png' WHERE `Id` = 341;
-- GUERRIER JULIETTE / HAVRE <- Juliette GUERRIER
UPDATE `players` SET `Photo` = 'guerrier-juliette-14534.png' WHERE `Id` = 342;
-- ROULY EMMA / NICE OGC <- Emma ROULY
UPDATE `players` SET `Photo` = 'rouly-emma-14568.png' WHERE `Id` = 343;
-- HERTOGHE ROSE / NICE OGC <- Rose Hertoghe
UPDATE `players` SET `Photo` = 'hertoghe-rose-29601.png' WHERE `Id` = 344;
-- SCHUPBACH LEA / PLAN-DE-CUQUES <- Léa Schupbach
UPDATE `players` SET `Photo` = 'schupbach-lea-29566.png' WHERE `Id` = 345;
-- NAAL LIYAH / PLAN-DE-CUQUES <- Liyah NAAL
UPDATE `players` SET `Photo` = 'naal-liyah-30062.png' WHERE `Id` = 346;
-- KPODAR DEBORAH / STELLA SAINT-MAUR <- Deborah Kpodar
UPDATE `players` SET `Photo` = 'kpodar-deborah-29626.png' WHERE `Id` = 347;
-- PESSOA MAYSSA / ST-AMAND <- Mayssa Pessoa
UPDATE `players` SET `Photo` = 'pessoa-mayssa-29621.png' WHERE `Id` = 348;
-- LOPES JHENNIFER / ST-AMAND <- Jhennifer Lopes
UPDATE `players` SET `Photo` = 'lopes-jhennifer-29620.png' WHERE `Id` = 349;
-- LOPEZ MARTA / ST-AMAND <- Marta Lopez Herrero
UPDATE `players` SET `Photo` = 'lopez-herrero-marta-29618.png' WHERE `Id` = 350;
-- AROUNIAN SANTOS MARCELA / ST-AMAND <- Marcela Arounian
UPDATE `players` SET `Photo` = 'arounian-marcela-29619.png' WHERE `Id` = 351;
-- EVA MBATA / ST-AMAND <- Eva MBATA
UPDATE `players` SET `Photo` = 'mbata-eva-23250.png' WHERE `Id` = 352;
-- HERANVAL MARIE / HAVRE <- Marie HERANVAL
UPDATE `players` SET `Photo` = 'heranval-marie-14075.png' WHERE `Id` = 354;
-- BOULOU SOLENNA / HAVRE <- Solenna BOULOU
UPDATE `players` SET `Photo` = 'boulou-solenna-30069.png' WHERE `Id` = 355;
-- AILINCAI ANDREEA / PARIS92 <- Andreea Ailincai
UPDATE `players` SET `Photo` = 'ailincai-andreea-29609.png' WHERE `Id` = 358;

COMMIT;

SELECT COUNT(*) AS players_total,
       SUM(CASE WHEN `Photo` IS NULL OR `Photo` = '' THEN 1 ELSE 0 END) AS players_without_photo
FROM `players`;
