-- Complète, pour les marrainages existants, les champs Marrainage.EtatCivil / Profession / Age
-- restés vides parce que le transfert depuis le questionnaire n'avait pas encore été branché sur
-- ces colonnes au moment de leur création (ils existent en tant que réponses "MarrainageAnswer"
-- génériques, jamais recopiées vers la fiche). Ne touche JAMAIS un champ déjà rempli — ne
-- fait qu'ajouter ce qui manque.
--
-- Limite connue : le questionnaire actuel demande l'"Âge", pas la date de naissance — il n'est
-- pas possible de reconstituer une date de naissance fiable à partir d'un âge sans connaître la
-- date exacte de la réponse. La "Date de naissance" ne peut donc être backfillée ici ; elle
-- devra être saisie à la main pour les fiches existantes. Pour les nouvelles inscriptions,
-- remplacez la question "Âge" par une question "Date de naissance" dans le gestionnaire de
-- questionnaire (menu Marrainages → outils admin) : le formulaire de création la reconnaît déjà.

UPDATE "Marrainages" m
SET "EtatCivil" = a."Value"
FROM "MarrainageAnswers" a
JOIN "MarrainageQuestions" q ON q."MarrainageQuestionId" = a."MarrainageQuestionId"
WHERE a."MarrainageId" = m."MarrainageId"
  AND (m."EtatCivil" IS NULL OR btrim(m."EtatCivil") = '')
  AND a."Value" IS NOT NULL AND btrim(a."Value") <> ''
  AND lower(btrim(q."Label")) IN ('etat civil', 'état civil');

UPDATE "Marrainages" m
SET "Profession" = a."Value"
FROM "MarrainageAnswers" a
JOIN "MarrainageQuestions" q ON q."MarrainageQuestionId" = a."MarrainageQuestionId"
WHERE a."MarrainageId" = m."MarrainageId"
  AND (m."Profession" IS NULL OR btrim(m."Profession") = '')
  AND a."Value" IS NOT NULL AND btrim(a."Value") <> ''
  AND lower(btrim(q."Label")) = 'profession';

UPDATE "Marrainages" m
SET "Age" = a."Value"::integer
FROM "MarrainageAnswers" a
JOIN "MarrainageQuestions" q ON q."MarrainageQuestionId" = a."MarrainageQuestionId"
WHERE a."MarrainageId" = m."MarrainageId"
  AND m."Age" IS NULL
  AND a."Value" IS NOT NULL AND btrim(a."Value") ~ '^\d+$'
  AND lower(btrim(q."Label")) IN ('age', 'âge');

-- Aperçu, après exécution, des fiches encore incomplètes malgré ce backfill (à compléter à la main) :
-- SELECT "MarrainageId", "Nom", "Prenom", "DateNaissance", "EtatCivil", "Profession"
-- FROM "Marrainages"
-- WHERE "DateNaissance" IS NULL OR "EtatCivil" IS NULL OR "Profession" IS NULL;
