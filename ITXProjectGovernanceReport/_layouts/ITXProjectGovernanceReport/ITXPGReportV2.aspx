﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ITXPGReportV2.aspx.cs"
    Inherits="ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport.ITXPGReportV2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ITX Project Governance Report</title>
    <link href="jquery-ui-1.8.4.css" rel="stylesheet" type="text/css" />
    <link href="reset.css" rel="stylesheet" type="text/css" />
    <!--[if IE 6]>
        <link href="jquery.ganttView.css" rel="stylesheet" type="text/css" />
    <![endif]-->
    <!--[if IE 7]>
        <link href="jquery.ganttView_ie7.css" rel="stylesheet" type="text/css" />
    <![endif]-->
    <!--[if IE 8]>
        <link href="jquery.ganttView.css" rel="stylesheet" type="text/css" />
    <![endif]-->
    <!--[if IE 9]>
        <link href="jquery.ganttView.css" rel="stylesheet" type="text/css" />
    <![endif]-->
    <style type="text/css">
        body
        {
            font-family: tahoma, verdana, helvetica;
            font-size: 0.8em;
            padding: 10px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div id="GroupConfigurationContainer" expandstatus="1">
        <span>
            <img src="/_layouts/ITXProjectGovernanceReport/expand.png" alt='' />
            Manage groups</span>
        <div id="GroupConfigurationHolder">
        </div>
    </div>
    <div class="clearfix">
    </div>
    <div id="RefreshContainer">
        <span onclick="window.location = window.location ;" style="cursor: pointer; color: Blue;">
            Refresh</span>
    </div>
    <div class="clearfix">
    </div>
    <asp:Label ID="JSONData" runat="server" CssClass="jsondata"></asp:Label>
    <div id="ganttChart">
    </div>

    <script type="text/javascript" src="jquery-1.4.2.js"></script>

    <script type="text/javascript" src="date.js"></script>

    <script type="text/javascript" src="jquery-ui-1.8.4.js"></script>

    <script type="text/javascript" src="jquery.ganttView_usercontrol.js"></script>

    <script type="text/javascript">
        $(function() {
            var data = new Array();
            var output = $.parseJSON($('.jsondata').html());
            if (output != null) {
                var projectseriesIndex = -1;
                var projectgroupIndex = -1;
                for (var i = 0; i < output.length; i++) {
                    if (output[i].Type == "Group") {
                        projectgroupIndex = projectgroupIndex + 1;
                        var hash = new Array();
                        hash["name"] = output[i].Title;
                        hash["projects"] = new Array();
                        data[projectgroupIndex] = hash;
                        projectseriesIndex = -1;
                    }
                    else if (output[i].Type == "Project") {
                        projectseriesIndex = projectseriesIndex + 1;
                        var hash = new Array();
                        hash["id"] = projectseriesIndex + 1;
                        hash["name"] = output[i].Title;
                        var series = new Array();
                        var thash = new Array();
                        var startdate = new Date(output[i].Start);
                        var finishdate = new Date(output[i].Finish);
                        thash["name"] = output[i].Title; // startdate.getDate() + "-" + (startdate.getMonth() + 1).toString() + "-" + startdate.getYear() + " to " + finishdate.getDate() + "-" + (finishdate.getMonth() + 1) + "-" + finishdate.getYear();
                        thash["start"] = new Date(output[i].Start);
                        thash["end"] = new Date(output[i].Finish);
                        series[0] = thash;
                        hash["series"] = series;
                        data[projectgroupIndex]["projects"][projectseriesIndex] = hash;
                    }
                    else {
                        var thash = new Array();
                        var startdate = new Date(output[i].Start);
                        var finishdate = new Date(output[i].Finish);
                        thash["name"] = output[i].Title;
                        thash["start"] = new Date(output[i].Start);
                        thash["end"] = new Date(output[i].Finish);
                        var length = data[projectgroupIndex]["projects"][projectseriesIndex]["series"].length;
                        data[projectgroupIndex]["projects"][projectseriesIndex]["series"][length] = thash;
                    }
                }
                $("#ganttChart").ganttView({
                    data: data,
                    slideWidth: 600,
                    behavior: {
                        onClick: function(data) {
                        },
                        onResize: function(data) {
                        },
                        onDrag: function(data) {
                        },
                        clickable: false,
                        draggable: false,
                        resizable: false
                    }
                });

                // For Project Level Collapse and Expand
                $('Div.ganttview-vtheader-item-name img').attr('collapsestatus', 1).click(function() {
                    if ($(this).attr('collapsestatus') == "1") {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/expand.png');
                        $(this).attr('collapsestatus', 0);
                        var projectname = $.trim($(this).parent().find('p').html());
                        $('.ganttview-block-container[projname="' + projectname + '"]').not(':first').hide();
                        $(this).parents('.ganttview-vtheader-item-name').height($(this).parents('.ganttview-vtheader-item').eq(0).find('.ganttview-vtheader-series').eq(0).find('..ganttview-vtheader-series-name').eq(0).height());
                        $(this).parents('.ganttview-vtheader-item-name').eq(0).next('.ganttview-vtheader-series').eq(0).find('.ganttview-vtheader-series-name').not(':first').slideUp(200);
                    }
                    else {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/collapse.png');
                        $(this).attr('collapsestatus', 1);
                        var projectname = $.trim($(this).parent().find('p').html());
                        $('.ganttview-block-container[projname="' + projectname + '"]').not(':first').show();
                        $(this).parents('.ganttview-vtheader-item-name').height($(this).parents('.ganttview-vtheader-item').eq(0).find('.ganttview-vtheader-series').eq(0).height());
                        $(this).parents('.ganttview-vtheader-item-name').eq(0).next('.ganttview-vtheader-series').eq(0).find('.ganttview-vtheader-series-name').not(':first').slideDown(200);
                    }
                }).mouseover(function() {
                    if ($(this).attr('collapsestatus') == "1") {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/collapse-hover.png');
                    }
                    else {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/expand-hover.png ');
                    }
                }).mouseout(function() {
                    if ($(this).attr('collapsestatus') == "1") {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/collapse.png');
                    }
                    else {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/expand.png ');
                    }
                });

                // For Group Level Collapse and Expand
                $('Div.ganttview-vtheader-item-name-group img').attr('collapsestatus', 1).click(function() {
                    if ($(this).attr('collapsestatus') == "1") {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/expand.png');
                        $(this).attr('collapsestatus', 0);
                        $(this).parents('.ganttview-vtheader-item').eq(0).find('.ganttview-vtheader-item-name p').each(function() {
                            var projectname = $.trim($(this).html());
                            $('.ganttview-block-container[projname="' + projectname + '"]').hide();
                        });
                        $(this).parents('.ganttview-vtheader-item').eq(0).find('.ganttview-vtheader-item-name,.ganttview-vtheader-series').slideUp(200);
                    }
                    else {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/collapse.png');
                        $(this).attr('collapsestatus', 1);
                        $(this).parents('.ganttview-vtheader-item').eq(0).find('.ganttview-vtheader-item-name p').each(function() {
                            var projectname = $.trim($(this).html());
                            if ($(this).prev().attr('collapsestatus') == "1") {
                                $('.ganttview-block-container[projname="' + projectname + '"]').show();
                            }
                            else {
                                $('.ganttview-block-container[projname="' + projectname + '"]').eq(0).show();
                            }
                        });
                        $(this).parents('.ganttview-vtheader-item').eq(0).find('.ganttview-vtheader-item-name,.ganttview-vtheader-series').slideDown(200);
                    }
                }).mouseover(function() {
                    if ($(this).attr('collapsestatus') == "1") {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/collapse-hover.png');
                    }
                    else {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/expand-hover.png ');
                    }
                }).mouseout(function() {
                    if ($(this).attr('collapsestatus') == "1") {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/collapse.png');
                    }
                    else {
                        $(this).attr('src', '/_layouts/ITXProjectGovernanceReport/expand.png ');
                    }
                });
            }
            else {
                $('#ganttChart').html("Report data not available. Contact your administrator for more details.");
            }
            /*
            setTimeout(getbackup, 3000);
            function getbackup() {
            $('.TempTextArea').val($('body').html());
            } */
        });
    </script>

    <script language="javascript" type="text/javascript">
        $(function() {
            function GroupOperations() {
                $('#GroupConfigurationContainer span').click(function() {
                    var parent = $(this).parent();
                    var status = parent.attr('expandstatus');
                    var This = $(this);
                    var gholder = $('#GroupConfigurationHolder');
                    if (status == "1") {
                        $.ajax({
                            type: 'GET',
                            url: '/_layouts/ITXProjectGovernanceReport/ProjectGroupConfig.aspx',
                            async: true,
                            cache: false,
                            beforeSend: function() {
                                This.unbind('click');
                                var waitimg = jQuery("<img>", { 'src': '/_layouts/ITXProjectGovernanceReport/wait.gif' });
                                waitimg.addClass('managegroupwait');
                                gholder.append(waitimg);
                                gholder.append(" Loading .........");
                            },
                            success: function(data) {
                                gholder.slideUp('fast').html(data).slideDown('fast');
                                parent.attr('expandstatus', '0');
                                parent.find('img').attr('src', '/_layouts/ITXProjectGovernanceReport/collapse.png');
                                //parent.height((gholder[0].scrollHeight + 30) + "px");
                                GroupOperations();
                            }
                        });
                    }
                    else {
                        gholder.slideUp(1000);
                        setTimeout(function() { gholder.empty(); }, 1000);
                        parent.attr('expandstatus', '1');
                        parent.find('img').attr('src', '/_layouts/ITXProjectGovernanceReport/expand.png');
                        parent.height('auto');
                    }
                }).css('cursor', 'pointer');
            }
            GroupOperations();
            $('#GroupConfigurationContainer span').mouseover(function() {
                var parent = $(this).parent();
                var status = parent.attr('expandstatus');
                var Img = $(this).find('img');
                if (status == "1") {
                    Img.attr('src', '/_layouts/ITXProjectGovernanceReport/expand-hover.png ');
                }
                else {
                    Img.attr('src', '/_layouts/ITXProjectGovernanceReport/collapse-hover.png');
                }
            }).mouseout(function() {
                var parent = $(this).parent();
                var status = parent.attr('expandstatus');
                var Img = $(this).find('img');
                if (status == "1") {
                    Img.attr('src', '/_layouts/ITXProjectGovernanceReport/expand.png ');
                }
                else {
                    Img.attr('src', '/_layouts/ITXProjectGovernanceReport/collapse.png');
                }
            });
        });
    </script>

    </form>
</body>
</html>