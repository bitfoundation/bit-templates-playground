# bit BlazorUI theme → Snapchat look-and-feel (experiment)

Goal: find out how far the bit BlazorUI theme system alone can move this app from **Fluent 2** to a **snapchat.com** look, and what still needs component-level or asset work.

**Sources:** values measured on 2026-09-15 from `www.snapchat.com` and `accounts.snapchat.com/v2/login`, using their CSS bundles and computed styles in headless Chrome.
**Where the theme lives:** a custom preset named `snapchat-light` / `snapchat-dark` in `src/Client/Bit.TemplatePlayground.Client.Core/Styles/_snapchat-theme.scss`.
**How it's switched on:** by the `bit-theme-light` / `bit-theme-dark` attributes on `<html>`.
**Verification:** a headless-Chrome script reads 60+ computed tokens on the running app in both schemes and checks the UI below (0 failures). Sign-in and home pages were screenshotted in light and dark.

Legend: `[x]` done and verified · `[~]` done with a deliberate deviation (see note) · `[ ]` open

> ⚠️ This is a local experiment. The Snapchat name, ghost logo and yellow are Snap Inc. trademarks. Do not ship or publish this build.

---

## 0. Wiring

- [x] Author the custom preset (`:root[bit-theme="snapchat-*"]` and its scoped `:root [bit-theme="snapchat-*"]` twin) in `Styles/_snapchat-theme.scss`, imported from `Styles/app.scss`. It covers all 398 tokens the Fluent 2 preset defines.
- [x] Point `bit-theme-light` / `bit-theme-dark` at `snapchat-light` / `snapchat-dark` in all 3 host pages (`Server.Web/Components/App.razor`, `Client.Web/wwwroot/index.html`, `Client.Maui/wwwroot/index.html`)
- [x] Light/dark still resolve. With the OS in light or dark, `<html>` gets `snapchat-light` or `snapchat-dark` respectively. Clicking the header theme toggle switches `snapchat-light` → `snapchat-dark` with no code change, because `ThemeService` treats any name ending in `dark` as dark.

## 1. Color – semantic roles (`--bit-clr-{role}-*`, 13 slots each; hover/active/dark/light steps derived with bit's `BitThemeColorDerivation`)

| Role | Snapchat meaning | Light main / on-color | Dark main / on-color |
| --- | --- | --- | --- |
| `pri` | Brand yellow (CTA, loader, selection) | `#FFFC00` / `#000000` | `#FFFC00` / `#000000` |
| `sec` | Action blue ("Next", "Log in", links) | `#0FADFF` / `#FFFFFF` | `#0FADFF` / `#FFFFFF` |
| `ter` | Ink button ("Download", "Cookie Menu") | `#121314` / `#FFFFFF` | `#FFFFFF` / `#121314` |
| `inf` | Neutral grey | `#656B73` | `#9A9FA7` |
| `suc` | Positive | `#157015` | `#4ECB4E` |
| `wrn` | Off-yellow | `#FFC131` | `#FFC131` |
| `swr` | No Snapchat equivalent, so orange was chosen | `#F77F00` | `#FF9F43` |
| `err` | Snap red | `#E1143D` | `#F23C57` |

- [~] `pri` – all 13 slots, light + dark. **Deviation:** yellow on white is 1.10:1, so it can't serve as text, stroke or glyph in light mode. Snapchat avoids that use, and so do we via the per-component overrides in §10.
- [~] `sec` – all 13 slots, light + dark. **Deviation:** Snapchat's white text on `#0FADFF` is 2.49:1, below bit's 4.5:1 on-color gate. Kept for fidelity.
- [x] `ter` – all 13 slots, light + dark
- [x] `inf` – all 13 slots, light + dark (5.38:1 / 7.89:1)
- [x] `suc` – all 13 slots, light + dark
- [x] `wrn` – all 13 slots, light + dark
- [~] `swr` – all 13 slots, light + dark. Snapchat has no severe-warning color, so this orange is our own choice.
- [x] `err` – all 13 slots, light + dark (4.81:1 / 5.56:1)
- [x] `-focus` for every role → `#000000` light / `#FFFFFF` dark (Snapchat never uses yellow as a focus indicator)

## 2. Color – neutral surfaces

| Token | Snapchat source | Light | Dark |
| --- | --- | --- | --- |
| `--bit-clr-bg-pri` | page / card surface | `#FFFFFF` | `#121314` (`black-v150`) |
| `--bit-clr-bg-sec` | login page wrapper | `#F7F7F7` | `#1E1E1E` (`surface-background`) |
| `--bit-clr-bg-ter` | `gray-v100` / `black-v100` | `#F0F1F2` | `#3A3E41` |
| `--bit-clr-bg-dis` | `buttonbg-disabled` | `#F0F1F2` | `#3A3E41` |
| `--bit-clr-bg-overlay` | `--modal-backdrop` | `rgba(0,0,0,.4)` | `rgba(0,0,0,.6)` |
| `--bit-clr-fg-pri` | `content-primary` | `#121314` | `#FFFFFF` |
| `--bit-clr-fg-sec` | `content-secondary` | `#53575B` | `#D4D5D6` |
| `--bit-clr-fg-ter` | field label | `#656B73` | `#9A9FA7` |
| `--bit-clr-fg-dis` | `gray-v300` | `#9A9FA7` | `#656B73` |
| `--bit-clr-brd-pri` | input border | `#E3E3E3` | `#53575B` |
| `--bit-clr-brd-sec` | card border | `#EBEBEB` | `#3A3E41` |
| `--bit-clr-brd-ter` | divider | `#F0F1F2` | `#202124` |
| `--bit-clr-brd-dis` | – | `#F0F1F2` | `#2A2C2F` |
| `--bit-clr-req` | red | `#E1143D` | `#F23C57` |

- [x] `bg-pri/sec/ter` (9-tone ramps + dis/dis-text/focus), light + dark
- [x] `fg-pri/sec/ter` (9-tone ramps + dis/dis-text/focus), light + dark. Every foreground clears 4.5:1 on all three light surfaces. In dark mode, `fg-ter` on `bg-ter` is 4.06:1.
- [~] `brd-pri/sec/ter` (9-tone ramps + dis/dis-text/focus), light + dark. **Deviation:** Snapchat's `#E3E3E3` field border is 1.28:1 on white, below bit's 3:1 non-text gate. Kept for fidelity; the focused field gets a 2px ink ring.
- [x] `bg-dis`, `fg-dis`, `brd-dis`, `bg-overlay`, `req`
- [~] Neutral grey ladder `--bit-clr-ntr-*` → Snapchat greys (`gray-v50 … black-v200`). `gray170` and `gray220` are interpolated.

## 3. Shadow / elevation (`--bit-shd-*`)

| Token | Snapchat source | Light value |
| --- | --- | --- |
| `--bit-shd-card` | login card (1px `#EBEBEB` border + soft drop) | `0 0 0 1px rgba(0,0,0,.08), 0 4px 8px rgba(0,0,0,.04)` |
| `--bit-shd-popup` | dropdown button | `0 12px 20px rgba(0,0,0,.12)` |
| `--bit-shd-dialog` | cookie modal (1px 20% border + drop) | `0 0 0 1px rgba(0,0,0,.2), 0 12px 20px rgba(0,0,0,.12)` |
| `--bit-shd-sheet` | share sheet | `0 8px 32px rgba(0,0,0,.25)` |
| `--bit-shd-tooltip` | tooltip | `0 2px 22px rgba(0,0,0,.1)` |
| `--bit-shd-snackbar` | blurred tile | `0 6px 12px 4px rgba(0,0,0,.1)` |
| `--bit-shd-appbar-top/bottom` | nav bar (flat) | `none` |
| `--bit-shd-inner` | – | `inset 0 1px 4px rgba(0,0,0,.1)` |

- [x] Per-surface elevations above, light + dark (dark = same geometry, stronger alpha, white hairline)
- [x] Size scale `sm, nm, md, lg, xl, 2xl`, plus `cal` / `cal2`, using Snapchat's own shadow set
- [x] Elevation steps `--bit-shd-1 … 24` mapped onto that scale

## 4. Shape (`--bit-shp-*`)

- [x] `brd-radius` / `radius-control` (inputs, dropdowns) → `8px`
- [x] `radius-button` → `var(--bit-shp-radius-full)` (pill buttons, measured at 45–100px)
- [x] `radius-chip` → `var(--bit-shp-radius-full)`
- [x] `radius-selection` (checkbox) → `4px`
- [x] `radius-surface` (cards) → `12px` (the most common card radius in Snapchat CSS)
- [x] `radius-popup` → `8px`
- [x] `radius-dialog` → `8px` (measured on the modal)
- [x] Radius scale `none/xs/sm/md/lg/xl/2xl/full` → `0/4/6/8/12/20/24/9999px`
- [x] `brd-width` `1px`, `brd-width-thick` `2px`

## 5. Focus

- [x] `--bit-shp-focus-ring-width` → `2px`
- [x] `--bit-shp-focus-ring-offset` → `0rem` (hugs the control like Snapchat's browser outline). **Lesson:** a unitless `0` silently breaks bit's `calc(offset + width)`, and the ring disappears. It must carry a unit.
- [x] Focus ring colour (the role `-focus` tokens) → ink black / white. Verified: `0 0 0 2px #000` light, `#FFF` dark.
- [x] Focused text field = ink ring instead of yellow, like Snapchat's 2px black `TextInput`. Needed a per-component override (`--bit-tfl-clr`), see §10.

## 6. Size (`--bit-siz-*`, spacing multiples so density still applies)

- [x] Control heights `ctrl-sm/md/lg` → `32 / 40 / 48px` ("Download" 32, buttons 40, inputs 48)
- [x] Control padding x `sm/md/lg` → `12 / 16 / 20px`; y → `4 / 8 / 12px`
- [x] `ctrl-min-width` → `auto` (Snapchat buttons hug their label)
- [x] Icons `icon-sm/md/lg` → `16 / 20 / 24px` (nav icons are 24)
- [x] Checkbox/radio `sel-sm/md/lg` → `16 / 20 / 24px`
- [x] List rows `item-sm/md/lg` → `36 / 44 / 52px` (44px language dropdown row)
- [x] `tab` → `48px`, `tab-indicator` → `2px`
- [x] `track-sm/md/lg` → `2 / 4 / 8px`, `spinner-stroke` → `3px`
- [x] Switch (iOS-style, as in the Snapchat app) md → `51 × 31px`, knob `27px`; sm `40 × 24 / 20`; lg `60 × 36 / 32`
- [x] Slider thumb `sm/md/lg` → `16 / 20 / 24px`
- [x] `dialog-max-width` → `33.75rem` (540px cookie modal)
- [~] 48px text inputs. `BitTextField` reads `ctrl-md` (shared with buttons), so this needed `--bit-tfl-min-height: var(--bit-siz-ctrl-lg)` at component level. Verified at 48px.

## 7. Spacing & layout

- [x] `--bit-spa-scaling-factor` → `0.5rem` (Snapchat's 4/8px grid, unchanged)
- [x] `--bit-spa-dialog` → `24px` (card padding)
- [x] `--bit-layout-density-scale` → `1`
- [x] Dialog actions → `row / center / center` (Snapchat centres its modal buttons)
- [x] Breakpoints `--bit-bp-*` → unchanged (Snapchat has no public breakpoint tokens)
- [x] Z-index `--bit-zin-*` → unchanged

## 8. Typography (`--bit-tpg-*`)

- [x] `font-family` → `"Avenir Next", "Nunito Sans", -apple-system, BlinkMacSystemFont, Roboto, "Segoe UI", Helvetica, Arial, sans-serif`
- [x] `font-family-mono` → `"SF Mono", "Fira Code", "Fira Mono", Menlo, Consolas, monospace`
- [x] Base `font-weight` (buttons, labels) → `600` (Avenir Next Demi Bold, Snapchat's most-used weight)
- [x] Weights `light/regular/medium/semibold/bold` → `300/400/500/600/700`
- [x] Size ramp `2xs…4xl` → `10/12/14/16/18/22/24/28/32px` (Snapchat's own sizes)
- [x] `h1…h6` → `32/28/24/22/18/16px`; `h2` = "Log in to Snapchat" (28/32px, 600)
- [x] `subtitle1/2`, `body1/2` (16/24 and 14/20, 400), `button` (14/20, 600), `caption1` (13/18, the field label size), `caption2` (11/14), `overline`
- [x] Control letter-spacing `normal`, text-transform `none`
- [~] Snapchat's second typeface **Graphik** (buttons, cookie UI) isn't applied. bit has one UI family token, with no separate heading or button family.

## 9. Motion & state

- [x] `mot-duration-short/normal/long` (`-full` sources) → `100 / 200 / 300ms` (Snapchat uses `.1s`, `.2s ease`, `.3s ease-in-out`)
- [x] `mot-easing` → CSS `ease`; decelerate → `ease-out`; accelerate → `ease-in`
- [x] `opa-dis` → `0.5` (no Snapchat data, so Fluent's value is kept)
- [x] Reduced motion, forced colours and high contrast safety nets keep working. Durations are set through the `-full` sources the reduced-motion rule reads. Built in; not separately browser-tested.

## 10. Beyond the theme tokens (brand assets & per-component)

- [x] **Font file:** Avenir Next is licensed and can't be redistributed. Apple devices render it natively. Everywhere else, **Nunito Sans** loads from Google Fonts (verified loaded; the CSP already allowed it).
- [x] **Logo:** Snapchat ghost SVG (`Client.Core/wwwroot/images/snapchat-ghost.svg`) in the `BitNavPanel` header and above the sign-in / sign-up forms. **Fix found in the loop:** in a column stack the image shrank to 9px, so it needs `flex-shrink: 0` (scoped `.brand-logo`).
- [x] **Favicon & PWA icon:** ghost on brand yellow. Replaced `favicon.ico` for Web, MAUI and Windows; added `images/icons/snapchat-icon-512.png` (manifest + apple-touch-icon).
- [~] **Native / PWA chrome:** `ThemeColors.cs` dark = `#121314`, `theme-color` meta tags in 3 host pages, `manifest.json` `background_color` = `#FFFC00` (yellow splash). The MAUI and Windows apps were not launched to check.
- [x] **Links:** Snapchat action blue, semibold, no underline (`.bit-lnk` in `app.scss`). bit links inherit text color, and no token exists for it.
- [~] **Icons:** kept Fabric MDL2. bit can render icons from external libraries, but Snapchat's outline icon set isn't publicly available.
- [x] **Button press feedback:** scale to 96% on press over `--bit-mot-duration-short` (`.bit-btn` in `app.scss`). No motion token for it.
- [x] **Found in the loop, yellow-on-white in light mode:** re-pointed to ink via component variables. Dark mode keeps yellow.
  - Outline/text primary buttons (`--bit-btn-clr`)
  - Text icons (`--bit-ico-clr`)
  - Tab indicator (`--bit-pvt-clr`)
  - Text-field accents (`--bit-tfl-clr`)
- [x] **Found in the loop, yellow-on-white in light mode (second pass):** same treatment, verified with no yellow-on-white left on the home and sign-in pages.
  - Nav-panel item icons (`--bit-nav-clr`)
  - Search-box magnifier (`--bit-srb-clr`)
  - Nav-panel footer action buttons (`--bit-acb-clr-ico`)

## 11. Verify

- [x] App builds (`dotnet build`, 0 errors)
- [x] Screenshots of sign-in and home, light + dark, compared with Snapchat's login page
- [x] Token check in the browser: 0 failures across both schemes
- [x] Flexibility verdict below

## 12. Round 2: layout clone (from side-by-side screenshots with snapchat.com)

Feedback: the search bars didn't match, nav bar text contrast was poor, and there was no "NEW" badge. Goal: get as close to Snapchat's layout as possible.

- [x] **Top consumer nav instead of a sidebar.** A 72px white bar with a hairline bottom border: ghost logo → search pill → icon-over-label destinations → round apps button → black pill actions (Sign in / Sign up for guests) → avatar. (`Header.razor`)
- [x] **Search bar** = Snapchat's borderless grey pill: 176×40px at x=72/y=16, where Snapchat's is 175×40 at the same spot. Typing suggests pages from the nav tree and Enter jumps to one. The nav drawer's own search box gets the same pill via `app.scss`.
- [x] **Nav items** = 24px glyph over a 12px demi-bold label: grey `#53575B` at rest, ink `#121314` when current. Replaces the yellow-on-white selected text.
- [x] **"NEW" badge** = Snapchat's blue `#0FADFF` pill with white text on the Todo item, via `BitNavBar`'s built-in `Badge`. Also shows on the mobile bottom bar.
- [x] **Sidebar → drawer at every width.** The apps button (desktop) or menu button (mobile) opens `BitNavPanel` as an off-canvas drawer with a scrim, and a click outside closes it. The page body now spans the full width.
- [x] **Mobile header** = Snapchat's round grey menu + search buttons next to a yellow pill (ghost + page title); the bottom bar is restyled the same as the desktop nav.
- [x] **Page title row** under the bar (24px semibold); account name hidden, avatar only.
- [x] **Current-page highlighting fixed.** The app's URLs carry the culture (`/en-US/categories`), which the navbar's exact match missed, so each item also lists its culture-prefixed URL (`AdditionalUrls`).

**What round 2 says about flexibility**
- **Component APIs covered most of it without CSS forks:** `BitNavBar` (badge, color, per-part `Classes`, header-bar mode), `BitSearchBox` (suggestions), `BitNavPanel` (open state, search), `BitImage` (`ImageFit`).
- **Layout-level, not theme-level:** turning a sidebar app into a top-nav app meant rewriting the header component. No theme can do that.
- **Gaps hit:**
  - `BitNavPanel` hard-codes its docked-vs-drawer switch at 960px, so making it a drawer on desktop meant re-declaring its small-screen CSS.
  - `BitImage`'s `Width`/`Height` size only the frame; without `ImageFit` an SVG shows cropped.
  - Culture-prefixed routes need `AdditionalUrls` for navbar selection.
- **Tooling:** the bit MCP tools answered every API question (navbar badge, search box, nav panel, image fit, icon names) without guessing. Hot reload applied the CSS but not the restructured Razor. Restarting the Aspire resource from an agent while the AppHost is IDE-launched failed, so the server was relaunched manually on its port.

## 13. Round 3: Snapchat+ style Home page (from snapchat.com/plus)

Feedback: bring something like snapchat.com/plus to the Home page, adapting as much as possible. The project may be changed freely as long as it builds, runs and can be demoed.

- [x] **Dark hero on a light app, using bit's scoped presets.** The hero is a `<section bit-theme="snapchat-dark">`, so every token inside it (text, buttons, focus, cards) is dark even while the app itself is in light mode. No component CSS was forked for this.
- [x] **Header darkens on Home only.** `<header bit-theme="snapchat-dark">` on Home, as snapchat.com/plus darkens its nav; the page-title row is hidden there.
- [x] **Warm backdrop** = Snapchat's measured `linear-gradient(#120D03 1%, #221E0F 39.86%, #211C10 100%)`.
- [x] **Gold neon ghost-plus logo** (142×142), drawn as our own SVG from the ghost outline with a gold gradient and glow (`images/snapchat-plus-logo.svg`), rather than copying Snapchat's image.
- [x] **Heading "Playground+"** 34/40px semibold and a subtitle at 16/24px medium grey, landing within 1px of Snapchat's positions (logo 614,98 vs 614,97; title y=254 vs 253; subtitle y=302 vs 301).
- [x] **Six perk chips** = `BitCard` with `Href`, an icon template and title/subtitle. `#322E20` surface, 12px corners, a gold 48px glyph chip, 16px title and 12px grey description, laid out in 2×343px columns (698px wide, identical to Snapchat) and 1 column on phones. Each links to a real feature: to-dos, dashboard, products, categories, team workspaces (tenants), settings.
- [x] **Chip height matches Snapchat's 76px.** Needed `BitCard`'s `Main`/`HeaderText` class slots to drop the card's own section paddings.
- [x] **CTA pill** = 343×52px yellow, "Get started" (→ sign up) for guests and "Open your dashboard" once signed in.
- [x] **Old Home headings and tenant card removed**; the statistics and bit product cards stay below the hero in the normal theme.

**What round 3 says about flexibility:** a scoped preset is the standout. One attribute turns any subtree into the dark palette, independent of the user's light/dark choice, and nested components follow automatically. `BitCard`'s class slots were enough to reshape it into a Snapchat chip without re-implementing it.

## Flexibility verdict

**The theme tier alone carried almost all of it.**
- **Everything a design system decides once is a token**, and all of it was re-valued to Snapchat's numbers without touching component CSS: the palette (8 roles × 13 slots, 9 neutral families), elevation, corner radii per component family, focus ring, control and glyph sizes, switch geometry, spacing, the type ramp and motion.
- **Palettes don't need hand-picking.** `BitThemeColorDerivation` turned Snapchat's handful of brand colors into full, contrast-reported ramps from a 60-line C# script.
- **Size:** one 716-line SCSS file, mostly generated. The existing light/dark toggle, SSR first paint, reduced-motion and high-contrast support all kept working unchanged.

**Where the theme ran out** (about 60 lines of component-level CSS in `app.scss`, plus assets):

1. **One color per role serves both fill and foreground.** Snapchat's yellow is a fill-only brand color. bit uses the role's main color for text, strokes and glyphs too, so light mode needed ink overrides on 7 components (button, icon, pivot, text field, search box, nav, action button). A per-role "foreground/on-surface" slot would remove this.
2. **Text fields share `ctrl-md` with buttons.** No separate input-height token, so 40px buttons and 48px fields needed a component variable.
3. **No link-colour token.** Links inherit text color.
4. **No press or scale motion token.**
5. **One UI font family.** Snapchat's display face (Graphik) can't be assigned to buttons or headings separately.
6. **Brand assets aren't themeable.** Logo, favicon, manifest and native chrome colors are separate files (and a `[mirror]` set).

**Accessibility trade-offs taken for fidelity:**
- Blue button text: 2.49:1.
- Input hairline border: 1.28:1.

bit's own gates flag both, and a production brand theme should fix them.
