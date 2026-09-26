-- Shows the output of executable C# cells in the rendered lecture.
--
-- Quarto does not run C# (engine: markdown), so the output shown under a
-- ```{.csharp} cell comes from the verified golden file expected/<name>.txt
-- that tools/verify_lectures.py records and CI checks. The golden is split on
-- its "── cell N ──" markers and each chunk is attached to the Nth executable
-- cell, in the same order tools/LectureRunner executes them.
--
-- Lectures without a golden (verify: run / none) render unchanged.

local function read_file(path)
  local f = io.open(path, "rb")
  if not f then
    return nil
  end
  local text = f:read("a")
  f:close()
  return (text:gsub("\r\n", "\n"))
end

local function trim(s)
  return (s:gsub("^%s+", ""):gsub("%s+$", ""))
end

-- Bodies of the ```{.csharp} fences in source order - the same cells
-- LectureRunner executes (plain ```csharp fences are illustration only).
local function executable_cells(source)
  local cells, current = {}, nil
  for line in (source .. "\n"):gmatch("(.-)\n") do
    if current then
      if line:match("^```%s*$") then
        table.insert(cells, trim(table.concat(current, "\n")))
        current = nil
      else
        table.insert(current, line)
      end
    elseif line == "```{.csharp}" then
      current = {}
    end
  end
  return cells
end

-- Golden output per cell number. Markers can land mid-line when a cell ends
-- with Console.Write, so split on the marker wherever it occurs.
local function outputs_by_cell(golden)
  local outputs = {}
  local marker = "── cell (%d+) ──"
  local pos, number = 1, nil
  while true do
    local s, e, n = golden:find(marker, pos)
    local chunk = golden:sub(pos, (s or 0) - 1)
    if number then
      outputs[number] = chunk
    end
    if not s then
      break
    end
    number, pos = tonumber(n), e + 1
  end
  return outputs
end

function Pandoc(doc)
  local input = quarto.doc.input_file
  local name = input:match("([^/\\]+)%.qmd$")
  local project = quarto.project.directory or pandoc.path.directory(input)
  local golden = name and read_file(pandoc.path.join({ project, "expected", name .. ".txt" }))
  local source = read_file(input)
  if not golden or not source then
    return nil
  end

  local cells = executable_cells(source)
  local outputs = outputs_by_cell(golden)
  local next_cell = 1

  return doc:walk({
    CodeBlock = function(block)
      if not block.classes:includes("csharp") or next_cell > #cells then
        return nil
      end
      if trim(block.text) ~= cells[next_cell] then
        return nil -- an illustration-only ```csharp fence
      end

      local output = outputs[next_cell]
      next_cell = next_cell + 1
      if not output then
        return nil
      end
      output = output:gsub("^\n+", ""):gsub("%s+$", "")
      if output == "" or output == "<volatile output masked>" then
        return nil
      end

      return pandoc.Div({
        block,
        pandoc.Div(
          -- Output lines are not code: no line numbers in the slide deck.
          { pandoc.CodeBlock(output, pandoc.Attr("", { "cell-output" }, { ["code-line-numbers"] = "false" })) },
          { class = "cell-output cell-output-stdout" }
        ),
      }, { class = "cell" })
    end,
  })
end
