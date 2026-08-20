# Terminal.Gui v2 Crash-Course (F#)

Notes on layout and main widgets, adapted for F#. Based on the official docs:
<https://tui-cs.github.io/Terminal.Gui/>

## Mental model

- One `View` tree. Containers (`Window`, `FrameView`, `Dialog`) hold subviews via `Add`.
- `app.Run(window)` blocks until the view quits (default: `Esc` or `Ctrl+Q`).
- Every view has 4 layout properties: `X`, `Y` (type `Pos`) and `Width`, `Height`
  (type `Dim`) — all relative to the **SuperView's content area**, never the screen.

Minimal app:

```fsharp
open Terminal.Gui.App
open Terminal.Gui.ViewBase
open Terminal.Gui.Views

[<EntryPoint>]
let main _ =
    use app = Application.Create()
    app.Init()

    let window = new Window(Title = "Hello World (Esc to quit)")
    let label =
        new Label(Text = "Hello, Terminal.Gui v2!", X = Pos.Center(), Y = Pos.Center())
    window.Add(label)

    app.Run(window)
    0
```

## F# gotchas

- Separate named property arguments with **commas**, not semicolons:
  `Label(Text = "x", X = Pos.Center())`. Semicolons produce
  "constructor takes 0 arguments" errors.
- Some views (e.g. `Label`) have a parameterless constructor; property
  assignment via constructor named args still works, but mutation is the
  always-compiles fallback:
  ```fsharp
  let label = new Label()
  label.Text <- "Hello"
  label.X <- Pos.Center()
  ```
- Use `new` (not bare `Type(...)`) for `IDisposable` types such as `Window`.
- C# `using` becomes `use`.

## Layout: the responsive model

Think CSS-ish, not grid coordinates. Ints are allowed (absolute), but the power
is in `Pos`/`Dim` expressions that re-resolve on terminal resize.

| Position (`Pos`)                        | Size (`Dim`)                    |
|----------------------------------------|---------------------------------|
| `Pos.Center()`                         | `Dim.Auto()` (fit content)      |
| `Pos.AnchorEnd()` (pin right/bottom)   | `Dim.Fill()` (remaining space)  |
| `Pos.Percent(20)`                      | `Dim.Fill(to: otherView)`       |
| `Pos.Right(v)` / `Pos.Bottom(v)`       | `Dim.Percent(50)`               |
| `Pos.Align(Alignment.End)`             | `Dim.Width(v)` (track a view)   |

`Pos`/`Dim` combine with `+`/`-`: `Pos.Center() - 10`, `Dim.Fill() - 2`.

`Frame` is the *resolved* rectangle after layout — read it for coordinates,
but declare layout with `Pos`/`Dim`.

Classic form-row pattern (label | stretching input | button):

```fsharp
let label = new Label(Text = "_Name:")          // _N = hotkey
let btn = new Button(Text = "_OK", X = Pos.AnchorEnd())
let field = new TextField(X = Pos.Right(label) + 1, Width = Dim.Fill(to = btn))
window.Add(label, field, btn)
```

### Adornments

Each view has `Margin` (outermost, shadows), `Border` (lines + `Title`),
`Padding` (innermost, scrollbars live here). Scrollbars are enabled via
`ViewportSettings` flags (`HasScrollBars`), rendered in the Padding.

## Main widgets

### Text / input

- `Label` — static text; hotkey jumps to the next view in `SubViews`.
- `TextField` — single-line editor.
- `TextView` — multiline editor.
- `TextValidateField` — masked/validated input.
- `NumericUpDown`, `DateEditor`, `TimeEditor`.

### Lists / tables

- `ListView` — scrollable list of strings.
- `ListView<T>` — typed; wraps `ObservableCollection<T>`, selected item via `.Value`.
- `TableView` — tabular data via `ITableSource` (the books grid).
- `TreeView<T>` — hierarchical data.

### Dropdowns / selection

- `DropDownList` — TextField + popover ListView combo.
- `OptionSelector<T>` — radio-style single pick.
- `CheckBox` — two/three states.
- `FlagSelector<T>` — `[Flags]` enums.

### Actions

`Button` fires `Accepting` (cancellable) then `Accepted` (committed):

```fsharp
btn.Accepted.Add(fun _ -> doSomething ())
```

### Containers / chrome

- `Window` — border + title.
- `FrameView` — bordered group box.
- `MenuBar` / `StatusBar` — `Shortcut`-based bars.
- `Tabs` — tabbed container (focused SubView = selected tab).
- `Dialog` / `Dialog<T>` — modal, buttons at bottom, result via `.Result`.

## Events & conventions

- Commit-style interaction uses `Accepting`/`Accepted` (v1's `Clicked` is gone).
- Selectors expose `SelectedChanged`-style events.
- `_` prefix in `Text` defines hotkeys (e.g. `"_OK"` → `Alt+O`).

## Drivers

Three built-in console drivers: `ansi` (default on Unix/macOS), `dotnet`
(cross-platform fallback via `System.Console`), `windows` (Win32 native).
Select explicitly with `app.Init(driverName)` or the `ForceDriver` config.
No web/WASM driver exists; to run in a browser, serve the app through a
terminal-over-WebSocket tool such as `ttyd`.

## booktracker usage map

Core views planned for this app:

- `MenuBar` — top navigation (Books / Hooks)
- `TableView` — book list
- `ListView` / `DropDownList` — status/rating filters
- `TextField` — search (`/`)
- Dialogs — add book (`a`), log reading session (`l`)
- `StatusBar` — keybinding hints

See README.md for the full keybinding table.
