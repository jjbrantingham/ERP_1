# Create UI Page Skill

## Purpose
Generate complete UI pages using Razor Pages or Blazor with Tailwind CSS styling for the ERP SaaS application.

## What to Create

### 1. Razor Page Model (Code-Behind)
**File**: `src/ERP.Web/Pages/[Feature]/[Page].cshtml.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using ERP.Application.[Context].Queries;
using ERP.Application.[Context].Commands;
using ERP.Application.[Context].DTOs;

namespace ERP.Web.Pages.[Feature];

[Authorize(Roles = "Administrator,[OtherRoles]")]
public class [Page]Model : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<[Page]Model> _logger;

    public [Page]Model(IMediator mediator, ILogger<[Page]Model> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty]
    public [CommandName] Input { get; set; }

    public PagedResult<[ResourceDto]> Items { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; }

    public async Task OnGetAsync()
    {
        Items = await _mediator.Send(new Search[Resources]Query
        {
            Page = CurrentPage,
            PageSize = 20,
            SearchTerm = SearchTerm
        });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var id = await _mediator.Send(Input);
            TempData["SuccessMessage"] = "[Resource] created successfully";
            return RedirectToPage("./Detail", new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating [resource]");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the [resource]");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id)
    {
        try
        {
            await _mediator.Send(new Delete[Resource]Command { Id = id });
            TempData["SuccessMessage"] = "[Resource] deleted successfully";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting [resource] {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the [resource]";
            return RedirectToPage();
        }
    }
}
```

### 2. Razor Page View
**File**: `src/ERP.Web/Pages/[Feature]/[Page].cshtml`

```cshtml
@page
@model [Feature].[Page]Model
@{
    ViewData["Title"] = "[Page Title]";
}

<div class="container mx-auto px-4 py-8">
    <!-- Page Header -->
    <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold text-gray-800">@ViewData["Title"]</h1>
        <button type="button"
                onclick="document.getElementById('createModal').classList.remove('hidden')"
                class="bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg shadow-md transition-colors">
            <i class="fas fa-plus mr-2"></i>
            Create New
        </button>
    </div>

    <!-- Success/Error Messages -->
    @if (TempData["SuccessMessage"] != null)
    {
        <div class="bg-green-100 border-l-4 border-green-500 text-green-700 p-4 mb-4 rounded" role="alert">
            <p class="font-medium">@TempData["SuccessMessage"]</p>
        </div>
    }
    @if (TempData["ErrorMessage"] != null)
    {
        <div class="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 mb-4 rounded" role="alert">
            <p class="font-medium">@TempData["ErrorMessage"]</p>
        </div>
    }

    <!-- Search & Filters -->
    <div class="bg-white shadow-md rounded-lg p-6 mb-6">
        <form method="get" class="flex gap-4">
            <div class="flex-1">
                <input type="text"
                       name="SearchTerm"
                       value="@Model.SearchTerm"
                       placeholder="Search..."
                       class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent">
            </div>
            <button type="submit"
                    class="bg-gray-600 hover:bg-gray-700 text-white font-semibold py-2 px-6 rounded-lg transition-colors">
                <i class="fas fa-search mr-2"></i>
                Search
            </button>
            <a asp-page="./[Page]"
               class="bg-gray-300 hover:bg-gray-400 text-gray-800 font-semibold py-2 px-6 rounded-lg transition-colors">
                Clear
            </a>
        </form>
    </div>

    <!-- Data Table -->
    <div class="bg-white shadow-md rounded-lg overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
                <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        [Column 1]
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        [Column 2]
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Status
                    </th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Actions
                    </th>
                </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
                @foreach (var item in Model.Items.Items)
                {
                    <tr class="hover:bg-gray-50 transition-colors">
                        <td class="px-6 py-4 whitespace-nowrap">
                            <div class="text-sm font-medium text-gray-900">@item.[Property1]</div>
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap">
                            <div class="text-sm text-gray-600">@item.[Property2]</div>
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap">
                            <span class="px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full
                                @(item.Status == "Active" ? "bg-green-100 text-green-800" :
                                  item.Status == "Pending" ? "bg-yellow-100 text-yellow-800" :
                                  "bg-gray-100 text-gray-800")">
                                @item.Status
                            </span>
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                            <a asp-page="./Detail" asp-route-id="@item.Id"
                               class="text-blue-600 hover:text-blue-900 mr-4">
                                <i class="fas fa-eye"></i> View
                            </a>
                            <a asp-page="./Edit" asp-route-id="@item.Id"
                               class="text-indigo-600 hover:text-indigo-900 mr-4">
                                <i class="fas fa-edit"></i> Edit
                            </a>
                            <button type="button"
                                    onclick="confirmDelete(@item.Id, '@item.[DisplayProperty]')"
                                    class="text-red-600 hover:text-red-900">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        </td>
                    </tr>
                }
            </tbody>
        </table>

        @if (!Model.Items.Items.Any())
        {
            <div class="text-center py-12">
                <i class="fas fa-inbox text-gray-400 text-5xl mb-4"></i>
                <p class="text-gray-500 text-lg">No items found</p>
            </div>
        }
    </div>

    <!-- Pagination -->
    @if (Model.Items.TotalPages > 1)
    {
        <div class="flex justify-center mt-6">
            <nav class="inline-flex rounded-md shadow-sm">
                @for (int i = 1; i <= Model.Items.TotalPages; i++)
                {
                    <a asp-page="./[Page]"
                       asp-route-currentPage="@i"
                       asp-route-searchTerm="@Model.SearchTerm"
                       class="px-4 py-2 border @(i == Model.CurrentPage ? "bg-blue-600 text-white border-blue-600" : "bg-white text-gray-700 border-gray-300 hover:bg-gray-50")
                              @(i == 1 ? "rounded-l-md" : "") @(i == Model.Items.TotalPages ? "rounded-r-md" : "")">
                        @i
                    </a>
                }
            </nav>
        </div>
    }
</div>

<!-- Create Modal -->
<div id="createModal" class="hidden fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
    <div class="relative top-20 mx-auto p-5 border w-11/12 md:w-3/4 lg:w-1/2 shadow-lg rounded-lg bg-white">
        <div class="flex justify-between items-center mb-4">
            <h3 class="text-2xl font-bold text-gray-800">Create [Resource]</h3>
            <button onclick="document.getElementById('createModal').classList.add('hidden')"
                    class="text-gray-400 hover:text-gray-600">
                <i class="fas fa-times text-2xl"></i>
            </button>
        </div>

        <form method="post">
            <div asp-validation-summary="ModelOnly" class="text-red-600 mb-4"></div>

            <div class="mb-4">
                <label asp-for="Input.[Property]" class="block text-sm font-medium text-gray-700 mb-2"></label>
                <input asp-for="Input.[Property]"
                       class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent" />
                <span asp-validation-for="Input.[Property]" class="text-red-600 text-sm"></span>
            </div>

            <div class="mb-4">
                <label asp-for="Input.[Property2]" class="block text-sm font-medium text-gray-700 mb-2"></label>
                <textarea asp-for="Input.[Property2]"
                          rows="3"
                          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"></textarea>
                <span asp-validation-for="Input.[Property2]" class="text-red-600 text-sm"></span>
            </div>

            <div class="flex justify-end gap-4 mt-6">
                <button type="button"
                        onclick="document.getElementById('createModal').classList.add('hidden')"
                        class="bg-gray-300 hover:bg-gray-400 text-gray-800 font-semibold py-2 px-6 rounded-lg">
                    Cancel
                </button>
                <button type="submit"
                        class="bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-6 rounded-lg">
                    Create
                </button>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script>
        function confirmDelete(id, name) {
            if (confirm(`Are you sure you want to delete "${name}"?`)) {
                fetch(`?handler=Delete&id=${id}`, {
                    method: 'POST',
                    headers: {
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    }
                }).then(() => window.location.reload());
            }
        }
    </script>
}
```

## Tailwind CSS Utility Classes Reference

### Layout
- `container mx-auto` - Centered container
- `px-4 py-8` - Padding (4 = 1rem, 8 = 2rem)
- `flex`, `grid` - Display
- `gap-4` - Gap between items
- `justify-between`, `justify-center` - Justify content
- `items-center` - Align items

### Typography
- `text-3xl`, `text-2xl`, `text-xl`, `text-lg`, `text-sm`, `text-xs` - Font size
- `font-bold`, `font-semibold`, `font-medium` - Font weight
- `text-gray-800`, `text-blue-600` - Text color
- `uppercase`, `lowercase` - Text transform

### Colors (Use project colors)
- `bg-white`, `bg-gray-50`, `bg-blue-600` - Background
- `text-gray-800`, `text-white` - Text
- `border-gray-300` - Border

### Borders & Shadows
- `border`, `border-2` - Border width
- `rounded-lg`, `rounded-md`, `rounded-full` - Border radius
- `shadow-md`, `shadow-lg` - Box shadow

### Spacing
- `m-4`, `mt-4`, `mb-4`, `mx-4`, `my-4` - Margin
- `p-4`, `pt-4`, `pb-4`, `px-4`, `py-4` - Padding

### Interactive States
- `hover:bg-blue-700` - Hover state
- `focus:ring-2 focus:ring-blue-500` - Focus state
- `transition-colors` - Smooth transitions

### Responsive Design
- `sm:`, `md:`, `lg:`, `xl:` - Breakpoint prefixes
- Example: `md:w-1/2` - Width 50% on medium screens and up

## Common UI Components

### Button Variants
```html
<!-- Primary -->
<button class="bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg">
    Primary
</button>

<!-- Secondary -->
<button class="bg-gray-600 hover:bg-gray-700 text-white font-semibold py-2 px-4 rounded-lg">
    Secondary
</button>

<!-- Danger -->
<button class="bg-red-600 hover:bg-red-700 text-white font-semibold py-2 px-4 rounded-lg">
    Delete
</button>

<!-- Outline -->
<button class="border-2 border-blue-600 text-blue-600 hover:bg-blue-600 hover:text-white font-semibold py-2 px-4 rounded-lg">
    Outline
</button>
```

### Form Input
```html
<input type="text"
       class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
       placeholder="Enter text..." />
```

### Alert Messages
```html
<!-- Success -->
<div class="bg-green-100 border-l-4 border-green-500 text-green-700 p-4 rounded" role="alert">
    Success message
</div>

<!-- Error -->
<div class="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 rounded" role="alert">
    Error message
</div>

<!-- Warning -->
<div class="bg-yellow-100 border-l-4 border-yellow-500 text-yellow-700 p-4 rounded" role="alert">
    Warning message
</div>
```

### Status Badges
```html
<span class="px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800">
    Active
</span>
```

## Accessibility Checklist

- [ ] Proper semantic HTML (header, nav, main, section)
- [ ] ARIA labels where needed
- [ ] Keyboard navigation support
- [ ] Focus indicators visible
- [ ] Alt text for images
- [ ] Form labels associated with inputs
- [ ] Error messages descriptive
- [ ] Color contrast meets WCAG standards

## Responsive Design Checklist

- [ ] Mobile-first approach
- [ ] Works on phones (< 640px)
- [ ] Works on tablets (640px - 1024px)
- [ ] Works on desktops (> 1024px)
- [ ] Touch-friendly buttons (min 44x44px)
- [ ] Readable text size on all devices
- [ ] Horizontal scrolling avoided

## Example Usage

```
User: /create-ui-page "Invoice List page with filters, pagination, and create modal"

Claude: I'll create a complete Invoice List page with Tailwind CSS...

[Creates .cshtml.cs and .cshtml files with full implementation]
```

## Related Skills
- create-api-endpoint
- create-cqrs-handlers
