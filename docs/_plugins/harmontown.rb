module Harmontown
  class EpisodeListPage < Jekyll::Page
    def initialize(site:, subDir:, field:, key:, title:, sequenceNumbers:, type: "group")
      @site = site             # the current site instance.
      @base = site.source      # path to the source directory.
      @dir  = 'episodes/' + subDir + '/' + Jekyll::Utils.slugify(type == "group" ? key : field)

      # All pages have the same filename, so define attributes straight away.
      @basename = 'index'      # filename without the extension.
      @ext      = '.html'      # the extension.
      @name     =  @basename + @ext

      @data = {
        'sequenceNumbers' => sequenceNumbers,
        'layout' => 'episode-list',
        'title' => title,
        'field' => field,
        'key' => key,
        'type' => type,
        'sitemap' => true,
        'isGenerated' => true
      }
    end

    # Placeholders that are used in constructing page URL.
    def url_placeholders
      {
        :path       => @dir,
        :category   => @dir,
        :basename   => basename,
        :output_ext => output_ext,
      }
    end
  end
end
