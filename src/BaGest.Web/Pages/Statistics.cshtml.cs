using BaGest.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace BaGest.Web;

public class StatisticsModel : PageModel
{
    private readonly IOptionsSnapshot<StatisticsOptions> _options;

    public StatisticsModel(IOptionsSnapshot<StatisticsOptions> options)
    {
        _options = options;
    }

    public IActionResult OnGet()
    {
        if (!_options.Value.EnableStatisticsPage) return NotFound();

        return Page();
    }
}
