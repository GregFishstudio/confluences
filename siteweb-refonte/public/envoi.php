<?php
// Formulaire de contact — redirige vers contact.html?envoi=ok|erreur
function retour($etat) {
    header("Location: contact.html?envoi=" . $etat . "#formulaire");
    exit;
}

if ($_SERVER["REQUEST_METHOD"] !== "POST") {
    retour("erreur");
}

// Piège à robots : ce champ caché doit rester vide
if (!empty($_POST["site"])) {
    retour("ok");
}

function propre($v) {
    $v = trim((string)($v ?? ""));
    // Empêche l'injection d'en-têtes
    return str_replace(["\r", "\n", "%0a", "%0d"], " ", $v);
}

$nom     = propre($_POST["nom"] ?? "");
$email   = propre($_POST["mail"] ?? "");
$tel     = propre($_POST["tel"] ?? "");
$objet   = propre($_POST["objet"] ?? "");
$message = trim((string)($_POST["message"] ?? ""));

if ($nom === "" || $message === "" || !filter_var($email, FILTER_VALIDATE_EMAIL)) {
    retour("erreur");
}

$destinataire = "confluences.vaud@gmail.com";
$sujet = "=?UTF-8?B?" . base64_encode("Site web — " . ($objet !== "" ? $objet : "Nouveau message")) . "?=";

$corps  = "Nom : $nom\n";
$corps .= "E-mail : $email\n";
$corps .= "Téléphone : $tel\n";
$corps .= "Objet : $objet\n\n";
$corps .= "Message :\n$message\n";

$entetes  = "From: Site Confluences <noreply@confluences.ch>\r\n";
$entetes .= "Reply-To: $email\r\n";
$entetes .= "Content-Type: text/plain; charset=UTF-8\r\n";

retour(mail($destinataire, $sujet, $corps, $entetes) ? "ok" : "erreur");
