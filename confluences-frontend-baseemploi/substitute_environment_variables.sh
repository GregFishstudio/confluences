#!/bin/sh
ROOT_DIR=/app
# Replace env vars in files served by NGINX
for file in $ROOT_DIR/js/*.js* $ROOT_DIR/index.html $ROOT_DIR/precache-manifest*.js;
do
  sed -i 's|VUE_APP_AUTHORITY_URL_PLACEHOLDER|'${VUE_APP_AUTHORITY_URL}'|g' $file
  sed -i 's|VUE_APP_APPLICATION_URL_PLACEHOLDER|'${VUE_APP_APPLICATION_URL}'|g' $file
  sed -i 's|VUE_APP_API_URL_PLACEHOLDER|'${VUE_APP_API_URL}'|g' $file
  # Your other variables here...
done

# Let container execution proceed
exec "$@"