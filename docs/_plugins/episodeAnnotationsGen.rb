module Harmontown
  class EpisodeAnnotationsGen < Jekyll::Generator
    def generate(site)

      prefixLength = '/srv/jekyll'.length

      episodes = site.collections['episodes'].docs.each { |ep|
        relative_url = ep.path[prefixLength..]
        ep.data['collection_item_url'] = relative_url
      }

      Jekyll.logger.info "EpisodeAnnotGen:", "Done."
    end
  end
end
