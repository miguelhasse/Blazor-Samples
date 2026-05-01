# Open Iconic assets in BlazorDualMode

This directory is a vendored copy of the Open Iconic distribution used by the `BlazorDualMode` sample. It exists so the hosted WebAssembly client can serve icon CSS and font files from its own static web root without taking a runtime dependency on a CDN.

## How the sample uses these files

- `Client\wwwroot\css\site.css` imports `open-iconic\font\css\open-iconic-bootstrap.min.css`
- `Client\Components\NavMenu.razor` uses classes such as `oi oi-home`, `oi oi-plus`, and `oi oi-list-rich`
- `Shared\Components\SurveyPrompt.razor` uses `oi oi-pencil`

Because the stylesheet expects fonts under `..\fonts\`, the directory layout under `open-iconic\font\` must stay intact if you replace or upgrade the asset bundle.

## Why this copy exists in the repo

The sample follows the asset layout used by the classic hosted Blazor templates, where icon font assets are served from `wwwroot` and referenced by relative CSS imports. Keeping the files local avoids external network dependencies and makes the sample self-contained for offline inspection.

## Upstream project and license

- Open Iconic home page: <http://useiconic.com/open>
- Icon SVG/CSS license: MIT
- Font license: SIL OFL

If you need the full icon catalog or the original upstream README, use the upstream Open Iconic site as the authoritative reference. This local README only documents how the assets are consumed inside `BlazorDualMode`.
