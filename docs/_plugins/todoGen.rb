module Harmontown
  class ToDoGenerator < Jekyll::Generator
    def generate(site)
      Jekyll.logger.info "ToDoGenerator:", "================ Start ================"

      indexPage = site.pages.find { |page| page.name == 'index.md' }
      indexPage.data['progress'] = Progress.new(site)

      Jekyll.logger.info "ToDoGenerator:", "================= End ================="
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
      [
        MissingValue.new('Date of live performance.', 'showDate', @episodes.select { |ep| ep['showDate'] == nil }.length),
        MissingValue.new('Location of live performance.', 'venue', @episodes.select { |ep| ep['venue'] == nil }.length),
        MissingValue.new('Episode description.', 'description', @episodes.select { |ep| ep['description'].strip.end_with? '...' }.length),
        MissingValue.new('Comptroller', 'comptroller', @episodes.select { |ep| ep['comptroller'] == nil }.length),
        MissingValue.new('Game Master', 'gameMaster', @episodes.select { |ep| ep['gameMaster'] == nil }.length),
        MissingValue.new('Whether episode has D&amp;D, Pathfinder, Shadowrun, or any other roleplaying session', 'hasDnD', @episodes.select { |ep| ep['hasDnD'] == nil }.length),
        MissingValue.new('List of guests.', 'guests', @episodes.select { |ep| ep['guests'] == nil }.length),
        MissingValue.new('List of audience members who participated.', 'audienceGuests', @episodes.select { |ep| ep['audienceGuests'] == nil }.length),
      ]
    end
  end

  class MissingValue < Liquid::Drop
    def initialize(name, field, count)
      @name = name
      @field = field
      @count = count
    end

    def name
      @name
    end

    def field
      @field
    end

    def count
      @count
    end
  end
end
