module Harmontown
  class EpisodeListGenerator < Jekyll::Generator
    def generate(site)
      Jekyll.logger.info "EpisodeListGenerator:", "================ Start ================"

      targetPage = site.pages.find { |page| page.path == 'episodes/index.html' }
      targetPage.data['byComptroller'] = Progress.new(site)
      
      episodes = site.collections['episodes'].docs

      targetPage.data['byComptroller'] = episodes.group_by { |ep| ep['comptroller'] }.map { |k,v| Grouping.new(k, v.length) }.sort_by { |item| [-item.count, item.by]}
      targetPage.data['byVenue'] = episodes.group_by { |ep| ep['venue'] }.map { |k,v| Grouping.new(k, v.length) }.sort_by { |item| [-item.count, item.by]}

      comptrollers = episodes.map { |ep| ep['comptroller']}.uniq
      gameMasters = episodes.map { |ep| ep['gameMaster']}.uniq
      guests = episodes.flat_map { |ep| ep['guests']}.uniq
      audienceGuests = episodes.flat_map { |ep| ep['audienceGuests']}.uniq
      people = comptrollers.chain(gameMasters).chain(guests).chain(audienceGuests).uniq
      
      byPerson = people.map { |person| Grouping.new(person, episodes.select { |ep| 
        ep['comptroller'] == person || 
        ep['gameMaster'] == person ||
        (ep['guests'] != nil && ep['guests'].include?(person)) ||
        (ep['audienceGuests'] != nil && ep['audienceGuests'].include?(person))
      }.length) }.sort_by { |item| [-item.count, item.by]}

      # todo: actually do
      targetPage.data['byPerson'] = byPerson

      Jekyll.logger.info "EpisodeListGenerator:", "================= End ================="
    end
  end

  class Grouping < Liquid::Drop
    def initialize(by, count)
      @by = by
      @count = count
    end

    def by
      @by
    end

    def count
      @count
    end
  end
end
