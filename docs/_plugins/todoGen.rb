module Harmontown
  class ToDoGenerator < Jekyll::Generator
    def generate(site)
      Jekyll.logger.warn "Junk:", "================ TEST ================"
      testPage = site.pages.find { |page| page.name == 'testing.html' }
      testPage.data['test'] = 99

    indexPage = site.pages.find { |page| page.name == 'index.md' }
    indexPage.data['progress'] = 99

    site.data.progress['episodeCount'] = 123
    end
  end
end
