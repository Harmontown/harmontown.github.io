module Harmontown
  class EpisodeAnnotationsGen < Jekyll::Generator
    def generate(site)

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
        
        previousEp = episodes[ep.data['sequenceNumber'] - 2]
        nextEp = episodes[ep.data['sequenceNumber']]

        ep.data['previousUrl'] = previousEp&.url
        ep.data['nextUrl'] = nextEp&.url
      }

      Jekyll.logger.info "EpisodeAnnotGen:", "Done."
    end
  end
end
