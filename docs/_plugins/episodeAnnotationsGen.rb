module Harmontown
  class EpisodeAnnotationsGen < Jekyll::Generator
    def generate(site)
      Jekyll.logger.info "EpisodeAnnotGen:", "Starting..."

      episodes = site.collections['episodes'].docs.sort_by { |ep| ep.data['sequenceNumber'] }
      
      prefixLength = '/srv/jekyll'.length
      episodes.each { |ep|
        collection_item_url = ep.path[prefixLength..]
        ep.data['collection_item_url'] = collection_item_url

        images_path = '/assets/images/episodes/' + ep['slug'].to_s.rjust(3, '0') + '/'
        ep.data['images'] = (ep.data['images'] || []) +
          site.static_files
            .select { |file| file.relative_path.start_with?(images_path) }
            .map { |file| file.relative_path }
      }

      Jekyll.logger.info "EpisodeAnnotGen:", "Done."
    end
  end
end
