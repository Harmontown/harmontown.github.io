module Harmontown
  class ToDoGenerator < Jekyll::Generator
    def generate(site)

      progress =  Progress.new(site)

      site.pages.concat(
        progress.missingValues.map { |missing| 
          EpisodeListPage.new(
            site: site, 
            subDir: 'missing',
            field: missing.field,
            key: nil, 
            title: "Episodes Missing " + missing.field,
            sequenceNumbers: missing.sequenceNumbers,
            type: 'missing') }
      )

      indexPage = site.pages.find { |page| page.name == 'index.md' }
      indexPage.data['progress'] = progress

      Jekyll.logger.info "ToDoGen:", "Done."
    end
  end

  class Progress < Liquid::Drop
    def initialize(site)
      @episodes = site.collections['episodes'].docs
    end

    def episodeCount
      @episodes.length
    end

    def missingValues
      def getSequenceNumbers(field)
        @episodes.select { |ep| ep[field] == nil }.map { |ep| ep['sequenceNumber']}
      end

      [
        MissingValue.new('Date of live performance.', 'showDate',
          getSequenceNumbers('showDate')),
        MissingValue.new('Location of live performance.', 'venue', 
          getSequenceNumbers('venue')),
        MissingValue.new('Episode description.', 'description', 
          @episodes.select { |ep| ep['description'].strip.end_with? '...' }.map { |ep| ep['sequenceNumber']}),
        MissingValue.new('Comptroller', 'comptroller',
          getSequenceNumbers('comptroller')),
        MissingValue.new('Game Master', 'gameMaster',
          getSequenceNumbers('gameMaster')),
        MissingValue.new('Whether episode has D&amp;D, Pathfinder, Shadowrun, or any other roleplaying session', 'hasDnD',
          getSequenceNumbers('hasDnD')),
        MissingValue.new('List of guests.', 'guests', 
          getSequenceNumbers('guests')),
        MissingValue.new('List of audience members who participated.', 'audienceGuests', 
          getSequenceNumbers('audienceGuests')),
        MissingValue.new('Custom episode image.', 'image', 
          @episodes.select { |ep| ep['image'] == '/assets/images/episode-placeholder.jpg' }.map { |ep| ep['sequenceNumber']}),
      ]
    end
  end

  class MissingValue < Liquid::Drop
    def initialize(name, field, sequenceNumbers)
      @name = name
      @field = field
      @sequenceNumbers = sequenceNumbers
    end

    def name
      @name
    end

    def field
      @field
    end

    def sequenceNumbers
      @sequenceNumbers
    end

    def count
      @sequenceNumbers.length
    end
  end
end
