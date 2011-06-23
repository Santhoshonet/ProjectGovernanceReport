<%@ Import Namespace="ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectGroupConfigure.ascx.cs"
    Inherits="ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport.ProjectGroupConfigure" %>
<link href="/_layouts/ITXProjectGovernanceReport/style.css" rel="stylesheet" type="text/css" />
<div class="ReportGroupByContainer">
    <div class="GroupNameContainer">
        <button class="top" stat="edit">
            Modify Group Names</button>
        <!-- This should appear when they click on the Modify group name button -->
        <!-- <button class="top">Save</button> or <a href="#" class="menu">Cancel</a> -->
        <ul>
            <!-- The text box should appear when they click on the Modify group button -->
            <!-- <li class="selected"><a href="#"><input type="text" name="GroupName"></a></li> -->
            <!-- <li><a href="#"><input type="text" name="GroupName">Group Name</a></li> -->
            <li class="selected notgroupedname"><a href="#" groupuid="<%= Guid.Empty %>">Not Grouped</a>
            </li>
            <asp:Repeater ID="RptrGroupnames" runat="server">
                <ItemTemplate>
                    <li><a href="#" groupuid="<%# DataBinder.Eval(Container.DataItem,"grpid").ToString()%>">
                        <%# DataBinder.Eval(Container.DataItem,"title").ToString()  %></a></li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
        <button class="bottom" stat="add">
            Add Group</button>
    </div>
    <!--! end of .GroupNameContainer -->
    <div class="ProjectNameContainer">
        <ul>
            <asp:Repeater ID="RptrProjectnames" runat="server">
                <ItemTemplate>
                    <li><span projectuid="<%# DataBinder.Eval(Container.DataItem,"uid").ToString()%>">
                        <img src="/_layouts/ITXProjectGovernanceReport/Drag.png" /><%# DataBinder.Eval(Container.DataItem,"name").ToString()%></span>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
    </div>
    <!--! end of .ProjectNameContainer -->
    <asp:Label CssClass="CurrentUserUID" runat="server" ID="LblCurUserUId" Style="display: none;
        visibility: hidden;"></asp:Label>
</div>
<div class="clearfix">
</div>
<% if (MyUtilities.IndividualPages)
   { %>
<div class="BottomMost clearfix">
    <label>
        click
        <asp:LinkButton ID="LnkConfigButton" runat="server">here</asp:LinkButton>
        to go project governance report.</label></div>
<% }
   else
   { %>
<div class="BottomMost">
</div>
<%  } %>

<script src="/_layouts/ITXProjectGovernanceReport/jquery-1.6.1.min.js" type="text/javascript"></script>

<script src="/_layouts/ITXProjectGovernanceReport/JQValidation.js" type='text/javascript'></script>

<script src="/_layouts/ITXProjectGovernanceReport/jquery-ui-1.8.13.custom.min.js"
    type="text/javascript"></script>

<script src="/_layouts/ITXProjectGovernanceReport/json2.js" type="text/javascript"></script>

<script type="text/javascript" language="javascript">
    // This hotfix makes older versions of jQuery UI drag-and-drop work in IE9
    (function($) { var a = $.ui.mouse.prototype._mouseMove; $.ui.mouse.prototype._mouseMove = function(b) { if ($.browser.msie && document.documentMode >= 9) { b.button = 1 }; a.apply(this, [b]); } } (jQuery));

    $(function() {
        function InitializeDraggable(elements) {
            elements.draggable({
                revert: true,
                //helper: 'clone',
                //distance: 10,
                cursor: 'move',
                opacity: 0.9,
                zindex: 999,
                create: function(event, ui) {
                },
                start: function(event, ui) {
                    var el = ui.helper;
                    el.css({ 'backgroundColor': '#fff9aa', 'cursor': "move" });
                },
                drag: function(event, ui) {

                },
                stop: function(event, ui) {
                    var el = ui.helper;
                    el.css({ 'backgroundColor': '#fff', 'cursor': "move" });
                }
            }).css({ 'cursor': "move" });
        }
        function InitializeDroppable(elements) {
            elements.droppable({
                tolerance: 'pointer',
                over: function(event, ui) {
                    $(this).css('backgroundColor', '#fff9aa');
                },
                out: function(event, ui) {
                    setlicolor($(this));
                },
                drop: function(event, ui) {
                    var group_li = event.target;
                    if (!$(group_li).hasClass('selected') && !$(group_li).hasClass('newgroupli') && !$(group_li).hasClass('editgroupli')) {
                        var project_li = ui.helper;
                        // Ajax code to send and create the group here
                        var project_li_clone = project_li.clone();
                        $.ajax({
                            type: 'POST',
                            url: '/_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx/PlaceProject',
                            data: "{'groupuid':'" + $(group_li).find('a').attr('groupuid') + "','projectuid':'" + project_li.find('span').attr('projectuid') + "','currentuseruid':'" + $('.CurrentUserUID').html() + "'}",
                            contentType: 'application/json; charset=utf-8',
                            async: true,
                            cache: false,
                            beforeSend: function() {
                                var waitimg = jQuery("<img>", { 'src': '/_layouts/ITXProjectGovernanceReport/wait.gif' });
                                waitimg.addClass('AddgroupWaitImg');
                                $(group_li).find('a').append(waitimg);
                                //ui.helper.remove();
                                ui.helper.hide();
                            },
                            success: function(data) {
                                $(group_li).find('img').remove();
                                EmptyProjectList();
                            },
                            dataType: 'json'
                        });
                    }
                    setlicolor($(this));
                }
            }).css({ 'cursor': 'pointer' });
            SetContainerHeight();
        }
        InitializeDraggable($('.ProjectNameContainer').find('ul li:visible'));

        InitializeDroppable($('.GroupNameContainer').find('ul li'));

        //---------- Group functions -------

        function EmptyProjectList() {
            // initally when the group is empty
            if ($('.ProjectNameContainer').find('ul li:visible').size() == 0) {
                var li = jQuery("<li>", {});
                li.append('<span><h3>This group is empty.</h3></span>').css({ 'text-align': 'center' });
                $('.ProjectNameContainer').find('ul').append(li);
            }
            SetContainerHeight();
        }
        EmptyProjectList();

        $('form').bind('keypress', function(e) {
            if (e.keyCode == 13) {
                return false;
            }
        });

        //-- Add Group functions --
        $('button.bottom').click(function() {
            if ($(this).attr('stat') == "add") {
                var li = jQuery("<li>", { 'class': 'newgroupli' });
                var ele_a = jQuery("<a>", { 'href': '#' });
                var input = jQuery("<input>", { 'type': 'text' });
                input.bind('keypress', function(e) {
                    if (e.keyCode == 13) {
                        $('button.bottom').not('.cancel').trigger('click');
                        return false;
                    }
                });
                ele_a.append(input);
                li.append(ele_a);
                $('div.GroupNameContainer ul').append(li);
                $(this).attr('stat', 'save');
                $(this).html("Save");
                $(this).after("<button class='bottom cancel' stat='cancel'>Cancel</button>");
                $('button.top').attr('disabled', true);
                input.focus();
                SetContainerHeight();
                doBgColorAnimation(li);
            }
            else {
                // Saving group here
                var inputbox = $('.newgroupli').find('input[type="text"]');
                if (inputbox.validateText({ errorclass: 'errorfocus' })) {
                    var groupname = $.trim(inputbox.val()).toString().toLowerCase();
                    // checking if the same group exists
                    var IsGroupAlreadyExists = false;
                    $('div.GroupNameContainer ul li a').each(function() {
                        if (groupname == $.trim($(this).html().toString().toLowerCase())) {
                            IsGroupAlreadyExists = true;
                        }
                    });
                    if (!IsGroupAlreadyExists) {
                        $('button.cancel').remove();
                        var This = $(this);
                        var waitimg = jQuery("<img>", { 'src': '/_layouts/ITXProjectGovernanceReport/wait.gif' });
                        waitimg.addClass('AddgroupWaitImg');
                        This.after(waitimg);
                        This.attr('disabled', true);
                        // Ajax code to send and create the group here
                        $.ajax({
                            type: 'POST',
                            url: '/_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx/AddGroup',
                            data: "{'gname':'" + inputbox.val() + "','currentuseruid':'" + $('.CurrentUserUID').html() + "'}",
                            contentType: 'application/json; charset=utf-8',
                            async: true,
                            cache: false,
                            success: function(data) {
                                var li = inputbox.parents('li').eq(0);
                                var a = inputbox.parent();
                                a.html(inputbox.val()).attr('groupuid', data.d);
                                li.append(a);
                                inputbox.remove();
                                This.attr('stat', 'add');
                                This.html("Add Group");
                                $('.AddgroupWaitImg').remove();
                                This.attr('disabled', false);
                                $('button.top').attr('disabled', false);
                                InitializeDroppable(li);
                                $('div.GroupNameContainer ul li').removeClass('newgroupli');
                            },
                            dataType: 'json'
                        });
                    }
                    else {
                        inputbox.addClass('errorfocus');
                        inputbox.focus();
                    }
                }
                SetContainerHeight();
            }
            return false;
        });
        // -- End Add Group functions ---

        // -- Edit Group functions --

        $('button.top').click(function() {
            var This = $(this);
            if (This.attr('stat') == "edit") {
                $('div.GroupNameContainer ul li').not('.notgroupedname').each(function() {
                    var inputbox = jQuery('<input>', { 'type': 'text' });
                    var groupname = $(this).find('a').html();
                    inputbox.val(groupname);
                    inputbox.attr('originalgroupname', groupname);
                    $(this).find('a').html('');
                    $(this).find('a').append(inputbox);
                    $(this).addClass('editgroupli');
                    $(this).css({ 'backgroundColor': '#fff9aa' });
                });
                This.attr('stat', 'update');
                This.html('Update');
                $('button.bottom').attr('disabled', true);
                This.after("<button class='top topcancel' stat='cancel'>Cancel</button>");
                doBgColorAnimation($('div.GroupNameContainer ul li').not('.notgroupedname'));
                SetContainerHeight();
            }
            else {
                var This = $(this);
                $('.topcancel').remove();
                var waitimg = jQuery("<img>", { 'src': '/_layouts/ITXProjectGovernanceReport/wait.gif' });
                waitimg.addClass('editgroupWaitImg');
                This.after(waitimg);
                This.attr('disabled', true);
                var modifiedList = [];
                var obj = [];
                obj[0] = $('.CurrentUserUID').html();
                obj[1] = $('.CurrentUserUID').html();
                modifiedList[modifiedList.length] = obj;
                $('div.GroupNameContainer ul li').not('.notgroupedname').find('input[type="text"]').each(function() {
                    var userText = $.trim($(this).val()).toLowerCase();
                    var originalText = $.trim($(this).attr('originalgroupname')).toLowerCase();
                    if (userText != originalText) {
                        obj = [];
                        obj[0] = $(this).parent().attr('groupuid');
                        obj[1] = $.trim($(this).val());
                        modifiedList[modifiedList.length] = obj;
                    }
                });
                if (modifiedList.length > 1) {
                    // Ajax code to send and modify the group names here
                    var data = JSON.stringify({ details: modifiedList });
                    $.ajax({
                        type: 'POST',
                        url: '/_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx/ModifyGroup',
                        data: data,
                        contentType: 'application/json; charset=utf-8',
                        async: true,
                        cache: false,
                        success: function(data) {
                            CancelEditGroupMode(data.d);
                        },
                        failure: function(e) {
                            CancelEditGroupMode(false);
                        },
                        dataType: 'json'
                    });
                }
                else
                    CancelEditGroupMode(false);
            }
            return false;
        });

        // -- End Of Edit Group functions

        // Loading Selected Group Projects Ajax
        LoadProjects($('div.GroupNameContainer ul li'));
        function LoadProjects(elements) {
            elements.live('click', function() {
                var This = $(this);
                if (!This.hasClass('selected') && !This.hasClass('newgroupli') && !This.hasClass('editgroupli')) {
                    var a_ele = $(this).find('a');
                    var groupuid = a_ele.attr('groupuid');
                    // Ajax code to send and modify the group names here
                    $.ajax({
                        type: 'POST',
                        url: '/_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx/GetProjects',
                        data: "{'groupuid':'" + groupuid + "','currentuseruid':'" + $('.CurrentUserUID').html() + "'}",
                        contentType: 'application/json; charset=utf-8',
                        async: true,
                        cache: false,
                        beforeSend: function() {
                            This.unbind('click');
                            $('div.GroupNameContainer ul li').removeClass('selected');
                            This.addClass('selected');
                            var proj_container = $('div.ProjectNameContainer ul');
                            proj_container.empty();
                            proj_container.append('<li><img src="/_layouts/ITXProjectGovernanceReport/wait.gif" />Loading...... </li>');
                        },
                        success: function(data) {
                            var proj_container = $('div.ProjectNameContainer ul');
                            proj_container.empty();
                            var Data = $.parseJSON(data.d);
                            if (Data.length > 0) {
                                for (var index = 0; index < Data.length; index++) {
                                    var li = jQuery("<li>", {});
                                    li.append('<span projectuid="' + Data[index].uid + '"><img src="/_layouts/ITXProjectGovernanceReport/Drag.png" />' + Data[index].name + '</span>');
                                    proj_container.append(li);
                                }
                                InitializeDraggable(proj_container.find('li'));
                            }
                            else {
                                EmptyProjectList();
                            }
                            This.addClass('selected');
                            resetColor();
                            LoadProjects(This);
                        },
                        failure: function(e) {
                            LoadProjects(This);
                        },
                        dataType: 'json'
                    });
                }
            });
        }

        // ---------- End Of Group Related functions -----------------

        // some common functions
        $('button.cancel').live('click', function() {
            $('.newgroupli').remove();
            $('button.bottom').attr('stat', 'add');
            $('button.bottom').html("Add Group");
            $('button.top').attr('disabled', false);
            $(this).remove();
            SetContainerHeight();
        });
        $('input[type="text"]').live('keydown', function() {
            $(this).removeClass('errorfocus');
        });
        $('button.topcancel').live('click', function() {
            CancelEditGroupMode(false);
            $(this).remove();
            SetContainerHeight();
        });
        function CancelEditGroupMode(status) {
            $('div.GroupNameContainer ul li').not('.notgroupedname').each(function() {
                var inputbox = $(this).find('a input');
                if (status)
                    $(this).find('a').html(inputbox.val());
                else
                    $(this).find('a').html(inputbox.attr('originalgroupname'));
                inputbox.remove();
                $(this).removeClass('editgroupli');
            });
            resetColor();
            $('button.top').attr('stat', 'edit');
            $('button.top').html("Modify Group Names");
            $('button.top').attr('disabled', false);
            $('button.bottom').attr('disabled', false);
            $('img.editgroupWaitImg').remove();
        }
        function setlicolor(element) {
            if (element.hasClass("newgroupli") || element.hasClass("editgroupli"))
                element.css('backgroundColor', '#fff9aa');
            else if (!element.hasClass("selected"))
                element.css('backgroundColor', '#fff');
            else
                element.css('backgroundColor', '#CACACA');
        }
        function resetColor() {
            $('div.GroupNameContainer ul li').css({ 'backgroundColor': '#fff' }).mousemove(function() {
                if (!$(this).hasClass('editgroupli')) {
                    $(this).css({ 'backgroundColor': '#CACACA' });
                }
            }).mouseout(function() {
                if (!$(this).hasClass('selected') && !$(this).hasClass('editgroupli')) {
                    $(this).css({ 'backgroundColor': '#fff' });
                }
            });
            $('div.GroupNameContainer ul li.selected').css({ 'backgroundColor': '#CACACA' });
        }

        // container if we put report in the same page
        function SetContainerHeight() {
            /*if ($('.ReportGroupByContainer').size() > 0) {
            var height = $('.ReportGroupByContainer')[0].scrollHeight + 30;
            $('#GroupConfigurationContainer').animate({ 'height': height }, { queue: false, duration: 500 });
            }*/
            if ($('.BottomMost').size() > 0) {
                var height = $('.BottomMost').offset().top;
                $('#GroupConfigurationContainer').animate({ 'height': height }, { queue: false, duration: 500 });
            }
        }

        function doBgColorAnimation(elements) {
            elements.animate({ backgroundColor: '#fff' }, 3000);
        }

        function GetJson(data) {
            if (typeof data !== "string" || !data) {
                return null;
            }
            // Make sure leading/trailing whitespace is removed (IE can't handle it)
            data = jQuery.trim(data);
            // Make sure the incoming data is actual JSON
            // Logic borrowed from http://json.org/json2.js
            if (/^[\],:{}\s]*$/.test(data.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, "@").replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, "]").replace(/(?:^|:|,)(?:\s*\[)+/g, ""))) {
                // Try to use the native JSON parser first
                return window.JSON && window.JSON.parse ? window.JSON.parse(data) : (new Function("return " + data))();
            } else {
                jQuery.error("Invalid JSON: " + data);
            }
        }
    });
</script>