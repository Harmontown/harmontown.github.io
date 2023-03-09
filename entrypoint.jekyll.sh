#!/bin/bash

# cd /srv/jekyll

rm -rf ./_site/
rm -rf ./.sass-cache/

echo '====================='
echo attempt 2
echo '====================='

exec jekyll serve -H 0.0.0.0 --livereload --livereload-port 35729 --force_polling --verbose --trace
