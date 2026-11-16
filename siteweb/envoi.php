<?php
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    // Fonction de nettoyage pour éliminer les balises HTML et les espaces indésirables
    function nettoyer_donnees($donnees) {
        $donnees = trim($donnees);
        $donnees = stripslashes($donnees);
        $donnees = htmlspecialchars($donnees);
        return $donnees;
    }

    // Récupérer et nettoyer les données du formulaire
    $nom = nettoyer_donnees($_POST["nom"]);
    $email = nettoyer_donnees($_POST["mail"]);
    $tel = nettoyer_donnees($_POST["tel"]);
    $objet = nettoyer_donnees($_POST["objet"]);
    $message = nettoyer_donnees($_POST["Message"]);

    // Valider l'adresse e-mail
    if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
        echo "Erreur: Veuillez fournir une adresse e-mail valide.";
        exit;
    }

    // Adresse e-mail de destination
    $destinataire = "confluences.vaud@gmail.com";

    // Sujet du message
    $sujet = "Nouveau message depuis le formulaire de contact";

    // Corps du message
    $corps_message = "Nom: $nom\n";
    $corps_message .= "Email: $email\n";
    $corps_message .= "Téléphone: $tel\n";
    $corps_message .= "Objet: $objet\n\n";
    $corps_message .= "Message:\n$message";

    // Envoi de l'e-mail
    $resultat = mail($destinataire, $sujet, $corps_message);

    // Vérifier si l'e-mail a été envoyé avec succès
    if ($resultat) {
        echo "Votre message a été envoyé avec succès.";
    } else {
        echo "Erreur lors de l'envoi du message. Veuillez réessayer plus tard.";
    }
} else {
    // Si la requête n'est pas de type POST, rediriger ou afficher un message d'erreur.
    echo "Erreur: Cette page ne peut être accédée directement.";
}
?>
