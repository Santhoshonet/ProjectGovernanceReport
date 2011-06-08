<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectGroupConfigure.ascx.cs"
    Inherits="ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport.ProjectGroupConfigure" %>
<link href="_layouts/ITXProjectGovernanceReport/style.css" rel="stylesheet" type="text/css" />
<div class="ReportGroupByContainer">
    <div class="GroupNameContainer">
        <button class="top" status="edit">
            Modify Group Names</button>
        <!-- This should appear when they click on the Modify group name button -->
        <!-- <button class="top">Save</button> or <a href="#" class="menu">Cancel</a> -->
        <ul>
            <!-- The text box should appear when they click on the Modify group button -->
            <!-- <li class="selected"><a href="#"><input type="text" name="GroupName"></a></li> -->
            <!-- <li><a href="#"><input type="text" name="GroupName">Group Name</a></li> -->
            <li class="selected notgroupedname"><a href="#">Not Grouped</a> </li>
            <asp:Repeater ID="RptrGroupnames" runat="server">
                <ItemTemplate>
                    <li><a href="#" groupuid="<%# DataBinder.Eval(Container.DataItem,"grpid").ToString()%>">
                        <%# DataBinder.Eval(Container.DataItem,"title").ToString()  %></a></li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
        <button class="bottom" status="add">
            Add Group</button>
    </div>
    <!--! end of .GroupNameContainer -->
    <div class="ProjectNameContainer">
        <ul>
            <asp:Repeater ID="RptrProjectnames" runat="server">
                <ItemTemplate>
                    <li><span projectuid="<%# DataBinder.Eval(Container.DataItem,"projid").ToString()%>">
                        <img src="_layouts/ITXProjectGovernanceReport/Drag.png" /><%# DataBinder.Eval(Container.DataItem,"title").ToString()%></span>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
    </div>
    <!--! end of .ProjectNameContainer -->
</div>

<script src="_layouts/ITXProjectGovernanceReport/jquery-1.6.1.min.js" type="text/javascript"></script>

<script src="_layouts/ITXProjectGovernanceReport/JQValidation.js" type='text/javascript'></script>

<script src="_layouts/ITXProjectGovernanceReport/jquery-ui-1.8.13.custom.min.js"
    type='text/javascript'></script>

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
                opacity: 0.5,
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
                    if (!$(this).hasClass("selected"))
                        $(this).css('backgroundColor', '#fff');
                    else
                        $(this).css('backgroundColor', '#CACACA');
                },
                drop: function(event, ui) {
                    var group_li = event.target;
                    if (!$(group_li).hasClass('selected')) {
                        var project_li = ui.helper;
                        // Ajax code to send and create the group here
                        var project_li_clone = project_li.clone();
                        $.ajax({
                            type: 'POST',
                            url: '_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx/PlaceProject',
                            data: "{'groupuid':'" + $(group_li).find('a').attr('groupuid') + "','projectuid':'" + project_li.find('span').attr('projectuid') + "'}",
                            contentType: 'application/json; charset=utf-8',
                            async: true,
                            cache: false,
                            beforeSend: function() {
                                var waitimg = jQuery("<img>", { 'src': '_layouts/ITXProjectGovernanceReport/wait.gif' });
                                waitimg.addClass('AddgroupWaitImg');
                                $(group_li).find('a').append(waitimg);
                                ui.helper.remove();
                            },
                            success: function(data) {
                                $(group_li).find('img').remove();
                            },
                            dataType: 'json'
                        });
                    }
                    if (!$(this).hasClass("selected"))
                        $(this).css('backgroundColor', '#fff');
                    else
                        $(this).css('backgroundColor', '#CACACA');
                }
            }).css({ 'cursor': 'pointer' });
        }
        InitializeDraggable($('.ProjectNameContainer').find('ul li'));

        InitializeDroppable($('.GroupNameContainer').find('ul li'));

        //---------- Group functions -------

        //-- Add Group functions --
        $('button.bottom').click(function() {
            if ($(this).attr('status') == "add") {
                var li = jQuery("<li>", { 'class': 'newgroupli' });
                var ele_a = jQuery("<a>", { 'href': '#' });
                var input = jQuery("<input>", { 'type': 'text' });
                ele_a.append(input);
                li.append(ele_a);
                $('div.GroupNameContainer ul').append(li);
                $(this).attr('status', 'save');
                $(this).html("Save");
                $(this).after("<button class='bottom cancel' status='cancel'>Cancel</button>");
                $('button.top').attr('disabled', true);
                input.focus();
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
                        var waitimg = jQuery("<img>", { 'src': '_layouts/ITXProjectGovernanceReport/wait.gif' });
                        waitimg.addClass('AddgroupWaitImg');
                        This.after(waitimg);
                        This.attr('disabled', true);
                        // Ajax code to send and create the group here
                        $.ajax({
                            type: 'POST',
                            url: '_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx/AddGroup',
                            data: "{'gname':'" + inputbox.val() + "'}",
                            contentType: 'application/json; charset=utf-8',
                            async: true,
                            cache: false,
                            success: function(data) {
                                var li = inputbox.parents('li').eq(0);
                                var a = inputbox.parent();
                                a.html(inputbox.val()).attr('groupuid', data.d);
                                li.append(a);
                                inputbox.remove();
                                This.attr('status', 'add');
                                This.html("Add Group");
                                $('.AddgroupWaitImg').remove();
                                This.attr('disabled', false);
                                $('button.top').attr('disabled', false);
                                InitializeDroppable(li);
                            },
                            dataType: 'json'
                        });
                    }
                    else {
                        inputbox.addClass('errorfocus');
                        inputbox.focus();
                    }
                }
            }
            return false;
        });
        // -- End Add Group functions ---

        // -- Edit Group functions --

        $('button.top').click(function() {
            var This = $(this);
            if (This.attr('status') == "edit") {
                $('div.GroupNameContainer ul li').not('.notgroupedname').each(function() {
                    var inputbox = jQuery('<input>', { 'type': 'text' });
                    var groupname = $(this).find('a').html();
                    inputbox.val(groupname);
                    inputbox.attr('originalgroupname', groupname);
                    $(this).find('a').html('');
                    $(this).find('a').append(inputbox);
                });
                This.attr('status', 'update');
                This.html('Update');
                $('button.bottom').attr('disabled', true);
                This.after("<button class='top topcancel' status='cancel'>Cancel</button>");
            }
            else {
                var This = $(this);
                $('.topcancel').remove();
                var waitimg = jQuery("<img>", { 'src': '_layouts/ITXProjectGovernanceReport/wait.gif' });
                waitimg.addClass('editgroupWaitImg');
                This.after(waitimg);
                This.attr('disabled', true);
                var modifiedList = [];
                $('div.GroupNameContainer ul li').not('.notgroupedname').find('input[type="text"').each(function() {
                    var userText = $.trim($(this).val()).toLowerCase();
                    var originalText = $.trim($(this).attr('originalgroupname')).toLowerCase();
                    if (userText != originalText) {
                        var obj = [];
                        obj[0] = $(this).parent().attr('groupuid');
                        obj[1] = userText;
                        modifiedList[modifiedList.length] = obj;
                    }
                });
                if (modifiedList.length > 0) {
                    // Ajax code to send and modify the group names here
                    $.ajax({
                        type: 'POST',
                        url: '_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx/ModifyGroup',
                        data: JSON.stringify({ details: modifiedList }),
                        contentType: 'application/json; charset=utf-8',
                        async: true,
                        cache: false,
                        success: function(data) {
                            CancelEditGroupMode();
                        },
                        failure: function(e) {
                            CancelEditGroupMode();
                        },
                        dataType: 'json'
                    });
                }
                else
                    CancelEditGroupMode();
            }
            return false;
        });

        // -- End Of Edit Group functions

        // Loading Selected Group Projects Ajax
        LoadProjects($('div.GroupNameContainer ul li'));
        function LoadProjects(elements) {
            $('div.GroupNameContainer ul li').live('click', function() {
                var This = $(this);
                if (!This.hasClass('selected')) {
                    var a_ele = $(this).find('a');
                    var groupuid = a_ele.attr('groupuid');
                    // Ajax code to send and modify the group names here
                    $.ajax({
                        type: 'POST',
                        url: '_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx/GetProjects',
                        data: "{'groupuid':'" + groupuid + "'}",
                        contentType: 'application/json; charset=utf-8',
                        async: true,
                        cache: false,
                        beforeSend: function() {
                            This.unbind('click');
                            $('div.GroupNameContainer ul li').removeClass('selected');
                            This.addClass('selected');
                            var proj_container = $('div.ProjectNameContainer ul');
                            proj_container.empty();
                            proj_container.append('<li><img src="_layouts/ITXProjectGovernanceReport/wait.gif" />Loading...... </li>');
                        },
                        success: function(data) {
                            var proj_container = $('div.ProjectNameContainer ul');
                            proj_container.empty();
                            var Data = $.parseJSON(data.d);
                            for (var index = 0; index < Data.length; index++) {
                                var li = jQuery("<li>", {});
                                li.append('<span projectuid="' + Data[index].uid + '"><img src="_layouts/ITXProjectGovernanceReport/Drag.png" />' + Data[index].name + '</span>');
                                proj_container.append(li);
                            }
                            InitializeDraggable(proj_container.find('li'));
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
            $('button.bottom').attr('status', 'add');
            $('button.bottom').html("Add Group");
            $('button.top').attr('disabled', false);
            $(this).remove();
        });
        $('input[type="text"]').live('keydown', function() {
            $(this).removeClass('errorfocus');
        });
        $('button.topcancel').live('click', function() {
            CancelEditGroupMode();
            $(this).remove();
        });
        function CancelEditGroupMode() {
            $('div.GroupNameContainer ul li').not('.notgroupedname').each(function() {
                var inputbox = $(this).find('a input');
                $(this).find('a').html(inputbox.attr('originalgroupname'));
                inputbox.remove();
            });
            $('button.top').attr('status', 'edit');
            $('button.top').html("Modify Group Names");
            $('button.top').attr('disabled', false);
            $('button.bottom').attr('disabled', false);
            $('img.editgroupWaitImg').remove();
        }

    });
</script>