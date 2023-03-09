#!/bin/bash
cd /src

rm -rf ./_site/
rm -rf ./.sass-cache/

exec bundle exec jekyll serve -H 0.0.0.0 --verbose --trace