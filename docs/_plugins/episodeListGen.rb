module Harmontown
  class EpisodeListGenerator < Jekyll::Generator
    def generate(site)

      episodes = site.collections['episodes'].docs
      def toGrouping (k, v)
        Grouping.new(k, v.map { |g| g['sequenceNumber'] })
      end
      def relevance (i)
        [-i.items.length, i.by]
      end
      def formatYesNoTbc (value)
        value == nil ? 'TBC' : value ? 'Yes' : 'No'
      end

      byComptroller =
        episodes.group_by { |ep| ep['comptroller'] }
          .map { |k,v| toGrouping(k || 'TBC',v) }
          .sort_by { |i| relevance(i) }


      byVenue =
        episodes.group_by { |ep| ep['venue'] }
          .map { |k,v| toGrouping(k || 'TBC',v) }
          .sort_by { |i| relevance(i) }


      comptrollers = episodes.map { |ep| ep['comptroller']}.uniq
      gameMasters = episodes.map { |ep| ep['gameMaster']}.uniq
      guests = episodes.flat_map { |ep| ep['guests']}.uniq
      audienceGuests = episodes.flat_map { |ep| ep['audienceGuests']}.uniq
      people = comptrollers.chain(gameMasters).chain(guests).chain(audienceGuests).uniq
      
      byPerson = people.map { |person| toGrouping(person || 'TBC', episodes.select { |ep| 
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
            site: site, 
            subDir: "with-comptroller",
            field: 'comptroller',
            key: grouping.by, 
            title: "Episodes with Comptroller " + grouping.by,
            sequenceNumbers: grouping.items) }
      )
      site.pages.concat(
        byVenue.map { |grouping| 
          EpisodeListPage.new(
            site: site, 
            subDir: "at-venue",
            field: 'venue',
            key: grouping.by, 
            title: "Episodes at Venue " + grouping.by,
            sequenceNumbers: grouping.items) }
      )
      site.pages.concat(
        byPerson.map { |grouping| 
          EpisodeListPage.new(
            site: site, 
            subDir: "with",
            field: 'person',
            key: grouping.by, 
            title: "Episodes with " + grouping.by, 
            sequenceNumbers: grouping.items) }
      )
      site.pages.concat(
        byDnD.map { |grouping| 
          EpisodeListPage.new(
            site: site, 
            subDir: "with-dnd",
            field: 'hasDnD',
            key: formatYesNoTbc(grouping.by),
            title: "Episodes with D&D: " + formatYesNoTbc(grouping.by),
            sequenceNumbers: grouping.items) }
      )

      # targetPage = site.pages.find { |page| page.path == 'episodes/index.html' }
      # targetPage.data['byComptroller'] = byComptroller
      # targetPage.data['byVenue'] = byVenue
      # targetPage.data['byPerson'] = byPerson
      # targetPage.data['byDnD'] = byDnD

      Jekyll.logger.info "EpisodeListGen:", "Done."
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
end
