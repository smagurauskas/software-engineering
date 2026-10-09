-- Fills a code block with code from a source file in the repository, so lecture
-- excerpts are the real (tested) code and cannot drift from it.
--
--   ```{.csharp include="07-testing/WordGame.UnitTests/GameServiceTests.cs" region="time"}
--   ```
--
-- `include` is relative to the project root. Without `region` the whole file is
-- included; with it, only the lines between `#region <name>` and the matching
-- `#endregion` markers (in any comment style, e.g. `// #region time`). Marker
-- lines of other regions inside the excerpt are dropped and the excerpt is
-- dedented. A missing file or region fails the render, so CI catches it.
--
-- The file path (without the leading demo folder) is shown above the code
-- unless the block sets its own `filename`.

local function read_lines(path)
  local f = io.open(path, "rb")
  if not f then
    return nil
  end
  local text = f:read("a"):gsub("\r\n", "\n"):gsub("^\239\187\191", "")
  f:close()
  local lines = {}
  for line in (text .. "\n"):gmatch("(.-)\n") do
    table.insert(lines, line)
  end
  return lines
end

local function region_name(line, keyword)
  return line:match("#" .. keyword .. "%s+([%w%-_]+)") or (line:match("#" .. keyword .. "%s*$") and "")
end

local function extract(lines, region)
  local out, depth, found = {}, 0, false
  for _, line in ipairs(lines) do
    local opening = line:match("#region") and region_name(line, "region")
    local closing = line:match("#endregion")
    if depth > 0 then
      if opening then
        depth = depth + 1
      elseif closing then
        depth = depth - 1
        if depth == 0 then
          break
        end
      else
        table.insert(out, line)
      end
    elseif opening == region then
      depth, found = 1, true
    end
  end
  return found and out or nil
end

local function strip_markers(lines)
  local out = {}
  for _, line in ipairs(lines) do
    if not (line:match("#region") or line:match("#endregion")) then
      table.insert(out, line)
    end
  end
  return out
end

local function dedent(lines)
  local indent
  for _, line in ipairs(lines) do
    if line:match("%S") then
      local current = #line:match("^%s*")
      indent = indent and math.min(indent, current) or current
    end
  end
  local out = {}
  for _, line in ipairs(lines) do
    table.insert(out, line:sub((indent or 0) + 1))
  end
  -- Drop leading and trailing blank lines
  while out[1] and not out[1]:match("%S") do
    table.remove(out, 1)
  end
  while out[#out] and not out[#out]:match("%S") do
    table.remove(out)
  end
  return out
end

function CodeBlock(block)
  local include = block.attributes["include"]
  if not include then
    return nil
  end

  local project = quarto.project.directory or pandoc.path.directory(quarto.doc.input_file)
  local lines = read_lines(pandoc.path.join({ project, include }))
  if not lines then
    error("include-region: cannot read " .. include)
  end

  local region = block.attributes["region"]
  local excerpt = region and extract(lines, region) or strip_markers(lines)
  if not excerpt then
    error("include-region: no region '" .. region .. "' in " .. include)
  end

  block.text = table.concat(dedent(excerpt), "\n")
  block.attributes["include"] = nil
  block.attributes["region"] = nil
  if not block.attributes["filename"] then
    block.attributes["filename"] = include:gsub("^[^/]+/", "")
  end
  return block
end
