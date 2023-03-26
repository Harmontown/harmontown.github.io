module Harmontown
  class ToDoGenerator < Jekyll::Generator
    def generate(site)
      Jekyll.logger.info "ToDoGen:", "Starting..."

      progress =  Progress.new(site)

      site.pages.concat(
        progress.missingValues.map { |missing| 
          EpisodeListPage.new(
            site: site, 
            subDir: 'missing',
            field: missing.field,
            key: nil, 
            title: "Episodes Missing ",
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
      def getSequenceNumbers(field, unset:nil, except:[])
        @episodes.select { |ep| ep[field] == unset && except.include?(ep['slug']) == false }.map { |ep| ep['sequenceNumber']}
      end

      [
        MissingValue.new('Date of live performance.', 'showDate',
          getSequenceNumbers('showDate', except:['nightsss', '361', '362'])),
        MissingValue.new('Location of live performance.', 'venue', 
          getSequenceNumbers('venue', except:['361', '362'])),
        MissingValue.new('Episode description.', 'description', 
          @episodes.select { |ep| ep['description'].strip.end_with? '...' }.map { |ep| ep['sequenceNumber']}),
        MissingValue.new('Comptroller', 'comptroller',
          getSequenceNumbers('comptroller', except:['nightsss', '361', '362'])),
        MissingValue.new('Game Master', 'gameMaster',
          getSequenceNumbers('gameMaster', except:['nightsss', '361', '362'])),
        MissingValue.new('Whether episode has D&amp;D, Pathfinder, Shadowrun, or any other roleplaying session', 'hasDnD',
          getSequenceNumbers('hasDnD')),
        MissingValue.new('List of guests.', 'guests', 
          getSequenceNumbers('guests')),
        MissingValue.new('List of audience members who participated.', 'audienceGuests', 
          getSequenceNumbers('audienceGuests')),
        MissingValue.new('Custom episode image.', 'image', 
          getSequenceNumbers('image', unset: '/assets/images/episode-placeholder.jpg', except:['nightsss', '361', '362'])),
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
