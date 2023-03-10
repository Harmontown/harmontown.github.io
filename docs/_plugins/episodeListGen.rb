module Harmontown
  class EpisodeListGenerator < Jekyll::Generator
    def generate(site)

      episodes = site.collections['episodes'].docs
      def toGrouping (k, v)
        Grouping.new(k || 'TBC', v.map { |g| g['sequenceNumber'] })
      end
      def relevance (i)
        [-i.items.length, i.by]
      end

      byComptroller =
        episodes.group_by { |ep| ep['comptroller'] }
          .map { |k,v| toGrouping(k,v) }
          .sort_by { |i| relevance(i) }


      byVenue =
        episodes.group_by { |ep| ep['venue'] }
          .map { |k,v| toGrouping(k,v) }
          .sort_by { |i| relevance(i) }


      comptrollers = episodes.map { |ep| ep['comptroller']}.uniq
      gameMasters = episodes.map { |ep| ep['gameMaster']}.uniq
      guests = episodes.flat_map { |ep| ep['guests']}.uniq
      audienceGuests = episodes.flat_map { |ep| ep['audienceGuests']}.uniq
      people = comptrollers.chain(gameMasters).chain(guests).chain(audienceGuests).uniq
      
      byPerson = people.map { |person| toGrouping(person, episodes.select { |ep| 
        ep['comptroller'] == person || 
        ep['gameMaster'] == person ||
        (ep['guests'] != nil && ep['guests'].include?(person)) ||
        (ep['audienceGuests'] != nil && ep['audienceGuests'].include?(person))
      }) }.sort_by { |i| relevance(i) }


      byDnD = [
        toGrouping(true, episodes.select { |ep| ep['hasDnD'] } ),
        toGrouping(false, episodes.select { |ep| ep['hasDnD'] == false } ),
        toGrouping(nil, episodes.select { |ep| ep['hasDnD'] == nil} ),
      ]


      site.pages.concat(
        byComptroller.map { |grouping| 
          EpisodeListPage.new(
            site, 
            "with-comptroller",
            'comptroller',
            grouping.by, 
            "Episodes with Comptroller ", grouping.items) }
      )
      site.pages.concat(
        byComptroller.map { |grouping| 
          EpisodeListPage.new(
            site, 
            "at-venue",
            'venue',
            grouping.by, 
            "Episodes at Venue ", grouping.items) }
      )
      site.pages.concat(
        byComptroller.map { |grouping| 
          EpisodeListPage.new(
            site, 
            "with",
            'person',
            grouping.by, 
            "Episodes with ", grouping.items) }
      )


      targetPage = site.pages.find { |page| page.path == 'episodes/index.html' }
      # targetPage.data['byComptroller'] = byComptroller
      # targetPage.data['byVenue'] = byVenue
      # targetPage.data['byPerson'] = byPerson
      targetPage.data['byDnD'] = byDnD

      Jekyll.logger.info "EpisodeListGenerator:", "Done."
    end
  end

  class Grouping < Liquid::Drop
    def initialize(by, items)
      @by = by
      @items = items
    end

    def by
      @by
    end

    def items
      @items
    end
  end

  class EpisodeListPage < Jekyll::Page
    def initialize(site, subDir, groupingField, of, titlePrefix, sequenceNumbers)
      @site = site             # the current site instance.
      @base = site.source      # path to the source directory.
      @dir  = 'episodes/' + subDir + '/' + Jekyll::Utils.slugify(of)

      # All pages have the same filename, so define attributes straight away.
      @basename = 'index'      # filename without the extension.
      @ext      = '.html'      # the extension.
      @name     =  @basename + @ext

      @data = {
        'sequenceNumbers' => sequenceNumbers,
        'layout' => 'episode-list',
        'titlePrefix' => titlePrefix,
        'groupingField' => groupingField,
        'of' => of,
        'sitemap' => true
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
