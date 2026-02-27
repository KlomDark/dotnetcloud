using DotNetCloud.Core.Models;

namespace DotNetCloud.Server.UI.Services
{
    public class TeamService
    {
        private readonly HttpClient _http;
        public TeamService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Team>> GetTeamsAsync()
            => await _http.GetFromJsonAsync<List<Team>>("api/Team");

        public async Task<Team> GetTeamAsync(System.Guid id)
            => await _http.GetFromJsonAsync<Team>($"api/Team/{id}");

        public async Task<Team> CreateTeamAsync(Team team)
        {
            var resp = await _http.PostAsJsonAsync("api/Team", team);
            return await resp.Content.ReadFromJsonAsync<Team>();
        }

        public async Task UpdateTeamAsync(System.Guid id, Team team)
            => await _http.PutAsJsonAsync($"api/Team/{id}", team);

        public async Task DeleteTeamAsync(System.Guid id)
            => await _http.DeleteAsync($"api/Team/{id}");

        public async Task AddUserToTeamAsync(System.Guid teamId, System.Guid userId)
            => await _http.PostAsync($"api/Team/{teamId}/users/{userId}", null);

        public async Task RemoveUserFromTeamAsync(System.Guid teamId, System.Guid userId)
            => await _http.DeleteAsync($"api/Team/{teamId}/users/{userId}");
    }
}
