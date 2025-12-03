#!/bin/sh

ROOT_DIR=/app

# Assurez-vous que les variables VITE_... sont définies 
# dans l'environnement du conteneur avant cet exec.

for file in $ROOT_DIR/js/*.js $ROOT_DIR/index.html $ROOT_DIR/precache-manifest.*js;
do
    # REMPLACER LES PLACEHOLDERS VUE_APP_ PAR LES VARIABLES VITE_
    
    # 1. Authority URL (IDP)
    sed -i "s|VUE_APP_AUTHORITY_URL_PLACEHOLDER|${VITE_AUTHORITY_URL}|g" $file
    
    # 2. Application URL (Frontend URL)
    sed -i "s|VUE_APP_APPLICATION_URL_PLACEHOLDER|${VITE_APPLICATION_URL}|g" $file
    
    # 3. API URL (Backend URL)
    sed -i "s|VUE_APP_API_URL_PLACEHOLDER|${VITE_API_URL}|g" $file
    
    # 4. URLs de Redirection (Optionnel, si vous les mettez en dur dans le .env)
    # Remplacer les placeholders de callback par les variables définies dans l'ENV
    sed -i "s|VUE_APP_CALLBACK_URL_PLACEHOLDER|${VITE_APPLICATION_URL}/callback|g" $file
    sed -i "s|VUE_APP_POST_LOGOUT_REDIRECT_URL_PLACEHOLDER|${VITE_APPLICATION_URL}|g" $file
    
    # Your other variables here...
done

exec "$@"