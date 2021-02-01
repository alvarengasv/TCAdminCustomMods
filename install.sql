INSERT INTO ar_common_configurations (id, moduleId, name, typeName, contents, app_data)
VALUES (1, 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0', 'UMod Configuration',
        'TCAdminCustomMods.Configurations.UModConfiguration, TCAdminCustomMods', '{}', '<?xml version="1.0" encoding="utf-16" standalone="yes"?>
<values>
  <add key="AR_COMMON:ConfigurationView" value="UModProvider" type="System.String,mscorlib" />
</values>');
INSERT INTO ar_common_configurations (id, moduleId, name, typeName, contents, app_data)
VALUES (2, 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0', 'Curse Configuration',
        'TCAdminCustomMods.Configurations.CurseConfiguration, TCAdminCustomMods', '{}', '<?xml version="1.0" encoding="utf-16" standalone="yes"?>
<values>
  <add key="AR_COMMON:ConfigurationView" value="CurseProvider" type="System.String,mscorlib" />
</values>');

# ---------------------------------------------------------------------------------------------------------------------

create table tcmodule_custom_mod_providers
(
    id                    int auto_increment
        primary key,
    name                  text        null,
    typeName              text        null,
    configurationModuleId varchar(36) null,
    configurationId       int         null,
    view                  text        null,
    app_data              text        null
);

# ---------------------------------------------------------------------------------------------------------------------

INSERT INTO tcmodule_custom_mod_providers (id, name, typeName, configurationModuleId, configurationId, view,
                                           app_data)
VALUES (1, 'uMod', 'TCAdminCustomMods.Providers.UModProvider, TCAdminCustomMods',
        'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0', 1, 'UModProvider', '');
INSERT INTO tcmodule_custom_mod_providers (id, name, typeName, configurationModuleId, configurationId, view,
                                           app_data)
VALUES (2, 'Curse', 'TCAdminCustomMods.Providers.CurseProvider, TCAdminCustomMods',
        'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0', 2, 'CurseProvider', '');

# ---------------------------------------------------------------------------------------------------------------------

INSERT INTO tc_page_icons (icon_id, module_id, page_id, linked_page_id, linked_page_module_id, display_name,
                           description, icon, url, display_sql, user_type, selected_user_type, view_order,
                           enabled, icon_manager, is_postback, postback_class, new_page, category_id)
VALUES (4892, 'd3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9', 23, 1, 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0',
        'Custom Mod Manager', 'Manage custom mods for $[Service.ConnectionInfo]', 'MenuIcons/GameHosting/Mods.png',
        '/Service/CustomMods/$[Service.ServiceId]', null, 0, null, 350, 1,
        'TCAdminCustomMods.IconManager, TCAdminCustomMods', null, null, null, null);
INSERT INTO tc_page_icons (icon_id, module_id, page_id, linked_page_id, linked_page_module_id, display_name,
                           description, icon, url, display_sql, user_type, selected_user_type, view_order,
                           enabled, icon_manager, is_postback, postback_class, new_page, category_id)
VALUES (4891, 'd3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9', 3, 2, 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0', 'Custom Mods',
        'Manage the custom mod providers this game supports', 'MenuIcons/GameHosting/Mods.png',
        '/Game/CustomModsAdmin/Index/$[Game.GameId]', null, 0, null, 975, 1, null, null, null, null, null);

# ---------------------------------------------------------------------------------------------------------------------

INSERT INTO tc_site_map (page_id, module_id, parent_page_id, parent_page_module_id, category_id, url, mvc_url,
                         controller, action, display_name, page_small_icon, panelbar_icon, show_in_sidebar,
                         view_order, required_permissions, menu_required_permissions, page_manager,
                         page_search_provider, cache_name)
VALUES (1, 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0', 23, 'd3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9', 2,
        '/Service/CustomMods/$[Service.ServiceId]', '/Service/CustomMods/$[Service.ServiceId]', 'CustomMods', 'Index',
        'Custom Mods', 'MenuIcons/GameHosting/Mods24x24.png', 'MenuIcons/GameHosting/Mods16x16.png', 0, 1000,
        '({07405876-e8c2-4b24-a774-4ef57f596384,0,0})({d3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9,5,0,1})({d3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9,6,0,2})({07405876-e8c2-4b24-a774-4ef57f596384,-1,64})({07405876-e8c2-4b24-a774-4ef57f596384,-1,2048})',
        '', null, null, '');
INSERT INTO tc_site_map (page_id, module_id, parent_page_id, parent_page_module_id, category_id, url, mvc_url,
                         controller, action, display_name, page_small_icon, panelbar_icon, show_in_sidebar,
                         view_order, required_permissions, menu_required_permissions, page_manager,
                         page_search_provider, cache_name)
VALUES (2, 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0', 3, 'd3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9', null,
        '/Game/CustomModsAdmin/Index/$[Game.GameId]', '/Game/CustomModsAdmin/Index/$[Game.GameId]', 'CustomModsAdmin',
        'Index', 'Custom Mods Administration', 'MenuIcons/GameHosting/Mods24x24.png',
        'MenuIcons/GameHosting/Mods16x16.png', 0, 1000,
        '({07405876-e8c2-4b24-a774-4ef57f596384,0})({d3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9,1})', '', null, null, '');

# ---------------------------------------------------------------------------------------------------------------------

INSERT INTO tc_modules (module_id, display_name, version, enabled, config_page, component_directory,
                        security_class)
VALUES ('b48cfbc9-7acc-4980-89c4-2b6a1f784aa0', 'Custom Mods', '2.0', 1, null, null, null);