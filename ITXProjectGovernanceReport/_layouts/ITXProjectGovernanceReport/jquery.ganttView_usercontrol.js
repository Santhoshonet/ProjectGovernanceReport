/*
jQuery.ganttView v.0.8.8
Copyright (c) 2010 JC Grubbs - jc.grubbs@devmynd.com
MIT License Applies
*/

/*
Options
-----------------
showWeekends: boolean
data: object
cellWidth: number
cellHeight: number
slideWidth: number
dataUrl: string
behavior: {
clickable: boolean,
draggable: boolean,
resizable: boolean,
onClick: function,
onDrag: function,
onResize: function
}
*/
(function(jQuery) {
    jQuery.fn.ganttView = function() {
        var args = Array.prototype.slice.call(arguments);

        if (args.length == 1 && typeof (args[0]) == "object") {
            build.call(this, args[0]);
        }

        if (args.length == 2 && typeof (args[0]) == "string") {
            handleMethod.call(this, args[0], args[1]);
        }
    };
    function build(options) {
        var els = this;
        var defaults = {
            showWeekends: true,
            cellWidth: 30,
            cellHeight: 31,
            slideWidth: 400,
            vHeaderWidth: 100,
            behavior: {
                clickable: true,
                draggable: true,
                resizable: true
            }
        };
        var opts = jQuery.extend(true, defaults, options);
        if (opts.data) {
            build();
        } else if (opts.dataUrl) {
            jQuery.getJSON(opts.dataUrl, function(data) { opts.data = data; build(); });
        }
        function build() {
            var minDays = Math.floor((opts.slideWidth / opts.cellWidth) + 5);
            var startEnd = DateUtils.getBoundaryDatesFromData(opts.data, minDays);
            opts.start = startEnd[0];
            opts.end = startEnd[1];
            els.each(function() {
                var container = jQuery(this);
                var div = jQuery("<div>", { "class": "ganttview" });
                new Chart(div, opts).render();
                container.append(div);
                var w = jQuery("div.ganttview-vtheader", container).outerWidth() +
					jQuery("div.ganttview-slide-container", container).outerWidth();
                container.css("width", (w + 2) + "px");
                new Behavior(container, opts).apply();
                // to fix ie7 css problem, setting height to ganttview-slide-container
                var ie7 = $.browser.msie && parseInt($.browser.version) === 7;
                if (ie7) {
                    var height = $('.ganttview').height() + 20;
                    $('.ganttview-slide-container').css({ "height": height + "px" });
                    $('.ganttview').css({ "height": height + "px" });
                }
            });
        }
    }
    function handleMethod(method, value) {
        if (method == "setSlideWidth") {
            var div = $("div.ganttview", this);
            div.each(function() {
                var vtWidth = $("div.ganttview-vtheader", div).outerWidth();
                $(div).width(vtWidth + value + 1);
                $("div.ganttview-slide-container", this).width(value);
            });
        }
    }
    var Chart = function(div, opts) {
        function render() {
            addVtHeader(div, opts.data, opts.cellHeight);
            var slideDiv = jQuery("<div>", {
                "class": "ganttview-slide-container",
                "css": { "width": opts.slideWidth + "px" }
            });
            dates = getDates(opts.start, opts.end);
            addHzHeader(slideDiv, dates, opts.cellWidth);
            addGrid(slideDiv, opts.data, dates, opts.cellWidth, opts.showWeekends);
            addBlockContainers(slideDiv, opts.data);
            addBlocks(slideDiv, opts.data, opts.cellWidth, opts.start);
            div.append(slideDiv);
            applyLastClass(div.parent());
        }
        var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        var graphcolors = ["#bbb", "#c3c3c3", "#ccc", "#d4d4d4", "#ddd", "#e5e5e5", "#eee", "#f6f6f6", "#fff"];
        var ReportHeader = "Project Governance Report";
        // Creates a 3 dimensional array [year][month][day] of every day
        // between the given start and end dates
        function getDates(start, end) {
            var dates = [];
            dates[start.getFullYear()] = [];
            dates[start.getFullYear()][start.getMonth()] = [start]
            var last = start;
            while (last.compareTo(end) == -1) {
                var next = last.clone().addDays(1);
                if (!dates[next.getFullYear()]) { dates[next.getFullYear()] = []; }
                if (!dates[next.getFullYear()][next.getMonth()]) {
                    dates[next.getFullYear()][next.getMonth()] = [];
                }
                dates[next.getFullYear()][next.getMonth()].push(next);
                last = next;
            }
            return dates;
        }
        function addVtHeader(div, data, cellHeight) {
            var headerDiv = jQuery("<div>", { "class": "ganttview-vtheader" });
            headerDiv.append("<p class='project-details-header'>" + ReportHeader + "</p>");
            for (var GroupIndex = 0; GroupIndex < data.length; GroupIndex++) {
                var itemDiv = jQuery("<div>", { "class": "ganttview-vtheader-item" });
                var groupTitle = jQuery("<div>", { "class": "ganttview-vtheader-item-name-group" });
                groupTitle.append("<img src='/_layouts/ITXProjectGovernanceReport/collapse.png' alt='' /><p>" + data[GroupIndex].name + "</p>");
                itemDiv.append(groupTitle);

                for (var i = 0; i < data[GroupIndex]["projects"].length; i++) {
                    itemDiv.append(jQuery("<div>", {
                        "class": "ganttview-vtheader-item-name",
                        "css": { "height": (data[GroupIndex]["projects"][i].series.length * cellHeight) + "px" }
                    }).append("<img src='/_layouts/ITXProjectGovernanceReport/collapse.png' alt='' /><p>" + data[GroupIndex]["projects"][i].name + "</p>"));
                    var seriesDiv = jQuery("<div>", { "class": "ganttview-vtheader-series" });
                    for (var j = 0; j < data[GroupIndex]["projects"][i].series.length; j++) {
                        var elem = jQuery("<div>", { "class": "ganttview-vtheader-series-name" });
                        elem.append(jQuery("<div>").append(data[GroupIndex]["projects"][i].series[j].name));
                        elem.append(jQuery("<div>").append("Start:" + data[GroupIndex]["projects"][i].series[j].start.getDate() + "-" + (data[GroupIndex]["projects"][i].series[j].start.getMonth() + 1).toString() + "-" + data[GroupIndex]["projects"][i].series[j].start.getYear() + "  End:" + data[GroupIndex]["projects"][i].series[j].end.getDate() + "-" + (data[GroupIndex]["projects"][i].series[j].end.getMonth() + 1).toString() + "-" + data[GroupIndex]["projects"][i].series[j].end.getYear()));
                        seriesDiv.append(elem);
                    }
                    itemDiv.append(seriesDiv);
                    headerDiv.append(itemDiv);
                }
            }
            div.append(headerDiv);
        }

        function addHzHeader(div, dates, cellWidth) {
            var headerDiv = jQuery("<div>", { "class": "ganttview-hzheader" });
            var monthsDiv = jQuery("<div>", { "class": "ganttview-hzheader-months" });
            var daysDiv = jQuery("<div>", { "class": "ganttview-hzheader-days" });
            var totalW = 0;
            for (var y in dates) {
                var noofmonths = 0;
                for (var mi in dates[y])
                    noofmonths += 1;
                var w = (noofmonths * cellWidth) + noofmonths;
                totalW = totalW + w;
                monthsDiv.append(jQuery("<div>", {
                    "class": "ganttview-hzheader-month",
                    "css": { "width": (w - 1) + "px" }
                }).append(y));
                for (var m in dates[y]) {
                    daysDiv.append(jQuery("<div>", { "class": "ganttview-hzheader-day" })
							.append(monthNames[parseInt(m, 0)]));
                }
            }
            monthsDiv.css("width", totalW + "px");
            daysDiv.css("width", totalW + "px");
            headerDiv.append(monthsDiv).append(daysDiv);
            div.append(headerDiv);
        }

        function addGrid(div, data, dates, cellWidth, showWeekends) {
            var gridDiv = jQuery("<div>", { "class": "ganttview-grid" });
            var rowDiv = jQuery("<div>", { "class": "ganttview-grid-row" });
            for (var y in dates) {
                for (var m in dates[y]) {
                    var cellDiv = jQuery("<div>", { "class": "ganttview-grid-row-cell" });
                    rowDiv.append(cellDiv);
                }
            }
            var w = (jQuery("div.ganttview-grid-row-cell", rowDiv).length * cellWidth) + jQuery("div.ganttview-grid-row-cell", rowDiv).length;
            rowDiv.css("width", w + "px");
            gridDiv.css("width", w + "px");
            for (var GroupIndex = 0; GroupIndex < data.length; GroupIndex++) {
                gridDiv.append(rowDiv.clone());
                for (var i = 0; i < data[GroupIndex]["projects"].length; i++) {
                    for (var j = 0; j < data[GroupIndex]["projects"][i].series.length; j++) {
                        gridDiv.append(rowDiv.clone());
                    }
                }
            }
            div.append(gridDiv);
        }

        function addBlockContainers(div, data) {
            var blocksDiv = jQuery("<div>", { "class": "ganttview-blocks" });
            for (var GroupIndex = 0; GroupIndex < data.length; GroupIndex++) {
                blocksDiv.append(jQuery("<div>", { "class": "ganttview-block-container" }));
                for (var i = 0; i < data[GroupIndex]["projects"].length; i++) {
                    for (var j = 0; j < data[GroupIndex]["projects"][i].series.length; j++) {
                        blocksDiv.append(jQuery("<div>", { "class": "ganttview-block-container", "projname": data[GroupIndex]["projects"][i].name }));
                    }
                }
            }
            div.append(blocksDiv);
        }

        function addBlocks(div, data, cellWidth, start) {
            var rows = jQuery("div.ganttview-blocks div.ganttview-block-container", div);
            var rowIdx = 0;
            graph_start = new Date(start);
            graph_start = graph_start.addDays(-graph_start.getDate());
            for (var GroupIndex = 0; GroupIndex < data.length; GroupIndex++) {
                rowIdx = rowIdx + 1;
                for (var i = 0; i < data[GroupIndex]["projects"].length; i++) {
                    for (var j = 0; j < data[GroupIndex]["projects"][i].series.length; j++) {
                        var series = data[GroupIndex]["projects"][i].series[j];
                        var size = DateUtils.daysBetween(series.start, series.end);
                        var noofmonths = DateUtils.monthsBetween(series.start, series.end);
                        var offset = DateUtils.daysBetween(graph_start, series.start);
                        var block = jQuery("<div>", {
                            "class": "ganttview-block",
                            "title": series.name + ", " + series.start.getDate() + "-" + (series.start.getMonth() + 1).toString() + "-" + series.start.getYear() + " to " + series.end.getDate() + "-" + (series.end.getMonth() + 1) + "-" + series.end.getYear() + ", " + size + " days",
                            "css": {
                                "width": (size - 2 + (noofmonths / 2)) + "px",
                                "margin-left": (offset - 1) + "px"
                            }
                        });
                        addBlockData(block, data[GroupIndex]["projects"][i], series);
                        //if (data[i].series[j].color) {
                        if (j < graphcolors.length)
                            block.css("background-color", graphcolors[j]);
                        else
                            block.css("background-color", graphcolors[graphcolors.length % j]);
                        //}
                        //block.append(jQuery("<div>", { "class": "ganttview-block-text" }).text(series.name + ", " + series.start.getDate() + "-" + (series.start.getMonth() + 1).toString() + "-" + series.start.getYear() + " to " + series.end.getDate() + "-" + (series.end.getMonth() + 1) + "-" + series.end.getYear() + ", " + size + " days"));
                        block.append(jQuery("<div>", { "class": "ganttview-block-text" }));
                        jQuery(rows[rowIdx]).append(block);
                        rowIdx = rowIdx + 1;
                    }
                }
            }
        }

        function addBlockData(block, data, series) {
            // This allows custom attributes to be added to the series data objects
            // and makes them available to the 'data' argument of click, resize, and drag handlers
            var blockData = { id: data.id, name: data.name };
            jQuery.extend(blockData, series);
            block.data("block-data", blockData);
        }

        function applyLastClass(div) {
            jQuery("div.ganttview-grid-row div.ganttview-grid-row-cell:last-child", div).addClass("last");
            jQuery("div.ganttview-hzheader-days div.ganttview-hzheader-day:last-child", div).addClass("last");
            jQuery("div.ganttview-hzheader-months div.ganttview-hzheader-month:last-child", div).addClass("last");
        }

        return {
            render: render
        };
    }

    var Behavior = function(div, opts) {
        function apply() {
            if (opts.behavior.clickable) {
                bindBlockClick(div, opts.behavior.onClick);
            }

            if (opts.behavior.resizable) {
                bindBlockResize(div, opts.cellWidth, opts.start, opts.behavior.onResize);
            }

            if (opts.behavior.draggable) {
                bindBlockDrag(div, opts.cellWidth, opts.start, opts.behavior.onDrag);
            }
        }

        function bindBlockClick(div, callback) {
            jQuery("div.ganttview-block", div).live("click", function() {
                if (callback) { callback(jQuery(this).data("block-data")); }
            });
        }

        function bindBlockResize(div, cellWidth, startDate, callback) {
            jQuery("div.ganttview-block", div).resizable({
                grid: cellWidth,
                handles: "e,w",
                stop: function() {
                    var block = jQuery(this);
                    updateDataAndPosition(div, block, cellWidth, startDate);
                    if (callback) { callback(block.data("block-data")); }
                }
            });
        }

        function bindBlockDrag(div, cellWidth, startDate, callback) {
            jQuery("div.ganttview-block", div).draggable({
                axis: "x",
                grid: [cellWidth, cellWidth],
                stop: function() {
                    var block = jQuery(this);
                    updateDataAndPosition(div, block, cellWidth, startDate);
                    if (callback) { callback(block.data("block-data")); }
                }
            });
        }

        function updateDataAndPosition(div, block, cellWidth, startDate) {
            var container = jQuery("div.ganttview-slide-container", div);
            var scroll = container.scrollLeft();
            var offset = block.offset().left - container.offset().left - 1 + scroll;

            // Set new start date
            var daysFromStart = Math.round(offset / cellWidth);
            var newStart = startDate.clone().addDays(daysFromStart);
            block.data("block-data").start = newStart;

            // Set new end date
            var width = block.outerWidth();
            var numberOfDays = Math.round(width / cellWidth) - 1;
            block.data("block-data").end = newStart.clone().addDays(numberOfDays);
            jQuery("div.ganttview-block-text", block).text(numberOfDays + 1);

            // Remove top and left properties to avoid incorrect block positioning,
            // set position to relative to keep blocks relative to scrollbar when scrolling
            block.css("top", "").css("left", "")
				.css("position", "relative").css("margin-left", offset + "px");
        }

        return {
            apply: apply
        };
    }

    var ArrayUtils = {
        contains: function(arr, obj) {
            var has = false;
            for (var i = 0; i < arr.length; i++) { if (arr[i] == obj) { has = true; } }
            return has;
        }
    };

    var DateUtils = {
        daysBetween: function(start, end) {
            if (!start || !end) { return 0; }
            start = Date.parse(start); end = Date.parse(end);
            if (start.getYear() == 1901 || end.getYear() == 8099) { return 0; }
            var count = 0, date = start.clone();
            while (date.compareTo(end) == -1) { count = count + 1; date.addDays(1); }
            return count;
        },
        monthsBetween: function(start, end) {
            if (!start || !end) { return 0; }
            start = Date.parse(start); end = Date.parse(end);
            if (start.getYear() == 1901 || end.getYear() == 8099) { return 0; }
            var count = 0, date = start.clone();
            while (date.compareTo(end) == -1) { count = count + 1; date.addMonths(1); }
            return count;
        },
        isWeekend: function(date) {
            return date.getDay() % 6 == 0;
        },

        getBoundaryDatesFromData: function(data, minDays) {
            var minStart = new Date(); maxEnd = new Date();
            for (var groupIndex = 0; groupIndex < data.length; groupIndex++) {
                for (var i = 0; i < data[groupIndex]["projects"].length; i++) {
                    for (var j = 0; j < data[groupIndex]["projects"][i].series.length; j++) {
                        var start = Date.parse(data[groupIndex]["projects"][i].series[j].start);
                        var end = Date.parse(data[groupIndex]["projects"][i].series[j].end)
                        if (j == 0 && i == 0 && groupIndex == 0) { minStart = start; maxEnd = end; }
                        if (minStart.compareTo(start) == 1) { minStart = start; }
                        if (maxEnd.compareTo(end) == -1) {
                            maxEnd = end;
                        }
                    }
                }
            }
            // Insure that the width of the chart is at least the slide width to avoid empty
            // whitespace to the right of the grid
            if (DateUtils.daysBetween(minStart, maxEnd) < minDays) {
                maxEnd = minStart.clone().addDays(minDays);
            }

            return [minStart, maxEnd];
        }
    };
})(jQuery);