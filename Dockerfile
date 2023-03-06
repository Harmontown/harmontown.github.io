FROM ruby:3-alpine

RUN apk add --no-cache build-base gcc bash cmake git

COPY /docs/Gemfile /

RUN bundle install

EXPOSE 4000 35729

COPY ./entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENTRYPOINT [ "/entrypoint.sh" ]