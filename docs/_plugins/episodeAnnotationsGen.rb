module Harmontown
  class EpisodeAnnotationsGen < Jekyll::Generator
    def generate(site)

      prefixLength = '/srv/jekyll'.length

      episodes = site.collections['episodes'].docs.each { |ep|
        relative_url = ep.path[prefixLength..]
        ep.data['collection_item_url'] = relative_url

        images_path = '/assets/images/episodes/' + ep['sequenceNumber'].to_s.rjust(3, '0') + '/'
        ep.data['images'] = (ep.data['images'] || []) +
          site.static_files
            .select { |file| file.relative_path.start_with?(images_path) }
            .map { |file| file.relative_path }
      }

      Jekyll.logger.info "EpisodeAnnotGen:", "Done."
    end
  end
end
