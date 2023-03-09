{% assign progress = site.data.progress %}

<p>Episode count: {{ progress.episodeCount }}</p>
<p>Junk: {{ page.episodeCount }}</p>
<table>
  <tr>
    <th>Value</th>
    <th># Missing</th>
    <th>% Complete</th>
  </tr>
  {% for item in progress.missingValues %}
  <tr>
    <td>{{ item.name }}</td>
    <td>{{ item.count }}</td>
    <td>{{ item.count | times: 100 | divided_by: progress.episodeCount }} %</td>
  </tr>
  {% endfor %}
</table>