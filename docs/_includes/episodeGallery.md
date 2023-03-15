
{% if page.images.size > 0 %}
<h2 class="gallery-section">Gallery ({{ page.images.size }} images)</h2>
<div class="episode-gallery">
  {% for file in page.images %}
  <div>
    <a href="{{ file }}" target="_blank"><img src="{{ file }}"/></a>
  </div>
  {% endfor %}
</div>
{% endif %}