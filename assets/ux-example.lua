-- Embeds playable examples of the word game into a lecture: the user interface of
-- the testing lecture ("before") next to the upgraded one ("after").
--
--   ::: {.ux-example name="wrong-word"}
--   :::
--
-- `name` is a scenario of 10-ui-ux/WordGame.Ux.Web/src/examples/scenarios.js.
-- Optional: show="before" or show="after" for one version only, height="400" (px),
-- before="..." and after="..." - what each version does, shown under its frame - and
-- try="...", principle="...", metric="..." - shown under the example in a row.
--
-- Every example is an iframe, so the styles and the keyboard of the game and of
-- the slides do not interfere. The examples need no server: the bundle
-- (`npm run build` in 10-ui-ux/WordGame.Ux.Web) contains both versions of the game
-- and a fake API. It is inlined once per document, so the slides stay one file.

local bundle_path = "10-ui-ux/WordGame.Ux.Web/dist-examples/examples.js"
local used = false

local captions = { before = "Before", after = "After" }

local function escape(text)
  return (text:gsub("&", "&amp;"):gsub("<", "&lt;"):gsub(">", "&gt;"):gsub('"', "&quot;"))
end

-- `note` says what this version does about the principle, right under its frame
local function frame(name, variant, height, note)
  local note_html = note and string.format('<p class="ux-note">%s</p>', escape(note)) or ""
  return string.format(
    '<figure class="ux-frame ux-%s"><figcaption>%s</figcaption>'
      .. '<iframe data-example="%s" data-variant="%s" title="%s: %s" style="height: %spx"></iframe>%s</figure>',
    variant, captions[variant], name, variant, captions[variant], name, height, note_html)
end

function Div(div)
  if not div.classes:includes("ux-example") then
    return nil
  end
  used = true

  local name = div.attributes["name"]
  if not name then
    error("ux-example: the `name` attribute is missing")
  end
  local show = div.attributes["show"] or "both"
  local height = div.attributes["height"] or "400"

  local frames = {}
  for _, variant in ipairs({ "before", "after" }) do
    if show == "both" or show == variant then
      table.insert(frames, frame(name, variant, height, div.attributes[variant]))
    end
  end
  local html = '<div class="ux-example">' .. table.concat(frames) .. "</div>"

  -- The same three facts under every example, always in the same place
  local facts = {}
  for _, fact in ipairs({ { "try", "Try" }, { "principle", "Principle" }, { "metric", "Metric" } }) do
    local value = div.attributes[fact[1]]
    if value then
      table.insert(facts, string.format("<div><dt>%s</dt><dd>%s</dd></div>", fact[2], escape(value)))
    end
  end
  if #facts > 0 then
    html = html .. '<dl class="ux-facts">' .. table.concat(facts) .. "</dl>"
  end
  return pandoc.RawBlock("html", html)
end

local function read_bundle()
  local project = quarto.project.directory or pandoc.path.directory(quarto.doc.input_file)
  local file = io.open(pandoc.path.join({ project, bundle_path }), "rb")
  if not file then
    local message = "ux-example: " .. bundle_path .. " is missing. "
      .. "Run `npm install` and `npm run build` in 10-ui-ux/WordGame.Ux.Web."
    -- CI must never publish slides with empty examples; locally the rest of the
    -- lecture can still be previewed without Node.js
    if os.getenv("CI") then
      error(message)
    end
    quarto.log.warning(message)
    return nil
  end
  local content = file:read("a")
  file:close()
  return content
end

local styles = [[
<style>
  .ux-example { display: flex; gap: 1.2rem; align-items: flex-start; justify-content: center; margin: 0.5rem 0; }
  .ux-example .ux-frame { flex: 1 1 0; max-width: 34rem; margin: 0; }
  .ux-example figcaption {
    font: 600 0.8rem system-ui, sans-serif; letter-spacing: 0.06em; text-transform: uppercase;
    text-align: left; margin: 0 0 0.3rem; color: #1a7f37;
  }
  .ux-example .ux-before figcaption { color: #b3261e; }
  .ux-example iframe {
    display: block; width: 100%; max-width: none; max-height: none; box-sizing: border-box;
    border: 1px solid #b8c0c8; border-top: 4px solid #1a7f37; border-radius: 6px; background: #f6f7f9;
  }
  .ux-example .ux-before iframe { border-top-color: #b3261e; }
  .ux-example .ux-note { margin: 0.35rem 0 0; text-align: left; font-size: 0.95rem; line-height: 1.3; }
  .reveal .ux-example .ux-note { font-size: 0.62em; }
  /* `.reveal dl.ux-facts` outranks the list styles of reveal.js */
  .ux-facts, .reveal dl.ux-facts {
    display: grid; grid-template-columns: 1fr 1.3fr 1fr; gap: 1.2rem;
    margin: 0.5rem 0 0; text-align: left; font-size: 0.9rem; line-height: 1.3;
  }
  .reveal dl.ux-facts { font-size: 0.6em; }
  .ux-facts > div { border-top: 2px solid #d0d7de; padding-top: 0.3rem; }
  .ux-facts dt, .reveal .ux-facts dt {
    font: 600 0.75em system-ui, sans-serif; letter-spacing: 0.06em; text-transform: uppercase;
    color: #55606b; margin: 0;
  }
  .ux-facts dd, .reveal .ux-facts dd { margin: 0; }
</style>
]]

-- Starts an example when it comes into view: when its slide is shown (reveal.js)
-- or when it is scrolled to (the page). Leaving a slide stops its examples, so
-- coming back starts them from the beginning.
local loader = [[
<script>
  (function () {
    const code = document.getElementById("ux-examples-bundle");
    const frames = Array.from(document.querySelectorAll(".ux-example iframe"));

    function start(frame) {
      if (frame.dataset.started) return;
      frame.dataset.started = "1";
      if (!code) {
        frame.srcdoc = "<p style='font: 16px system-ui; padding: 1rem'>The examples are not built: run <code>npm run build</code> in 10-ui-ux/WordGame.Ux.Web.</p>";
        return;
      }
      const example = JSON.stringify({ name: frame.dataset.example, variant: frame.dataset.variant });
      // The bundle stays base64 until it runs: script text inside HTML would have to be escaped
      frame.srcdoc =
        '<!doctype html><html lang="en"><head><meta charset="utf-8">' +
        '<meta name="viewport" content="width=device-width, initial-scale=1"></head><body><script>' +
        "window.uxExample = " + example + ";" +
        '(0, eval)(new TextDecoder().decode(Uint8Array.from(atob("' + code.textContent.trim() + '"), (c) => c.charCodeAt(0))));' +
        "<\/script></body></html>";
    }

    function stop(frame) {
      if (!frame.dataset.started) return;
      delete frame.dataset.started;
      frame.removeAttribute("srcdoc");
      frame.src = "about:blank";
    }

    function giveKeyboardBack() {
      if (document.activeElement && document.activeElement.tagName === "IFRAME") document.activeElement.blur();
      window.focus();
    }

    function setUp() {
      if (window.Reveal) {
        const sync = () => {
          const current = Reveal.getCurrentSlide();
          frames.forEach((frame) => (current && current.contains(frame) ? start(frame) : stop(frame)));
        };
        Reveal.on("slidechanged", sync);
        if (Reveal.isReady()) sync();
        else Reveal.on("ready", sync);
      } else {
        const observer = new IntersectionObserver((entries) =>
          entries.forEach((entry) => entry.isIntersecting && start(entry.target)), { rootMargin: "200px" });
        frames.forEach((frame) => observer.observe(frame));
      }
    }

    // In a slide deck this script runs before reveal.js is loaded
    if (document.querySelector(".reveal")) window.addEventListener("load", setUp);
    else setUp();

    // Sent by src/examples/main.jsx from inside the frames
    window.addEventListener("message", (event) => {
      const message = event.data && event.data.uxExample;
      if (message === "ready" && window.Reveal) giveKeyboardBack();
      if (message !== "key") return;
      const key = event.data.key;
      // The deck takes the keyboard back first, so the next key goes to it directly
      giveKeyboardBack();
      if (!window.Reveal || key === "Escape") return;
      if (key === "PageDown" || key === "ArrowRight" || key === "ArrowDown") Reveal.next();
      else if (key === "PageUp" || key === "ArrowLeft" || key === "ArrowUp") Reveal.prev();
    });
  })();
</script>
]]

function Pandoc(doc)
  if not used then
    return nil
  end
  local bundle = read_bundle()
  local html = styles
  if bundle then
    html = html .. '<script type="text/plain" id="ux-examples-bundle">' .. quarto.base64.encode(bundle) .. "</script>"
  end
  doc.blocks:insert(pandoc.RawBlock("html", html .. loader))
  return doc
end
