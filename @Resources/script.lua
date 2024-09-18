
WeekDayMap = {
    [0] = "Sunday",
    [1] = "Monday",
    [2] = "Tuesday",
    [3] = "Wednesday",
    [4] = "Thursday",
    [5] = "Friday",
    [6] = "Saturday"
}


function ProcessFile(filename, op)
    local file = io.open(filename, "r")
    local idx = 0
    for line in file:lines() do
        if line == "" then
            break
        end
        local contribs, date, weekNum = line:match("([^,]+),([^,]+),([^,]+)")
        local obj = {
            contribs = tonumber(contribs),
            date = date,
            weekNum = tonumber(weekNum),
            idx = idx
        }
        op(obj)
        idx = idx + 1
    end
    file:close()
end

function Debug(dayEntry)
    print("idx: " .. dayEntry.idx)
    print(dayEntry.contribs)
    print(dayEntry.date)
    print(dayEntry.weekNum)
end

function BuildSquare(dayEntry)
    -- Set the color of the square based on contributions
    -- Debug(dayEntry)
    local contribs = dayEntry.contribs
    local color
    if contribs == 0 then
        color = "#EmptyColor#"
    elseif contribs < 4 then
        color = "#Color1#"
    elseif contribs < 6 then
        color = "#Color2#"
    elseif contribs < 8 then
        color = "#Color3#"
    else
        color = "#Color4#"
    end

    local squareMeter = "Square" .. dayEntry.idx
    local radius = 5
    SKIN:Bang("!WriteKeyValue", squareMeter, "Meter", "Shape")
    SKIN:Bang("!WriteKeyValue", squareMeter, "Shape",
              "Rectangle " .. 0 .. "," .. 0 .. "," .. SquareSize .. "," .. SquareSize .."," .. radius .. "," .. radius ..  
              "| Fill Color " .. color .. "| StrokeWidth 0")

    SKIN:Bang("!WriteKeyValue", squareMeter, "MeterStyle", "SquareStyle")
    SKIN:Bang('!WriteKeyValue', squareMeter, 'ToolTipText', dayEntry.date .. " " .. WeekDayMap[dayEntry.weekNum])
    SKIN:Bang('!WriteKeyValue', squareMeter, 'MouseOverAction',
              '[!SetOption Active Text "Contributions: ' .. contribs ..'"][!UpdateMeter Active][!Redraw]')
    if dayEntry.weekNum % 7 == 0 then
        SKIN:Bang('!WriteKeyValue', squareMeter, 'X', '0R')
        SKIN:Bang('!WriteKeyValue', squareMeter, 'Y', '#Spacing#')
    else
        SKIN:Bang('!WriteKeyValue', squareMeter, 'X', '0r')
        SKIN:Bang('!WriteKeyValue', squareMeter, 'Y', '0R')
    end
    if dayEntry.idx == 0 then
        local x = SKIN:GetMeter('Background'):GetX()
        SKIN:Bang('!WriteKeyValue', squareMeter, 'X', x + Spacing)
        local y = dayEntry.weekNum * (SquareSize + Spacing) + Spacing
        SKIN:Bang('!WriteKeyValue', squareMeter, 'Y', y)
    end
end

function Initialize()
    print("Initializing")
    local dataFile = SKIN:GetVariable('dataPath')
    SquareSize = SKIN:GetVariable('SquareSize')
    Spacing = SKIN:GetVariable('Spacing')
    ProcessFile(dataFile, BuildSquare)
end