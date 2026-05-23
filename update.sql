INSERT IGNORE INTO ar_common_configurations (id, moduleId, name, typeName, contents, app_data)
VALUES (5, 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0', 'Nexus Mods Configuration',
        'TCAdminCustomMods.Configurations.NexusModsConfiguration, TCAdminCustomMods', '{}', '<?xml version="1.0" encoding="utf-16" standalone="yes"?>
<values>
  <add key="AR_COMMON:ConfigurationView" value="NexusModsProvider" type="System.String,mscorlib" />
</values>');

# ---------------------------------------------------------------------------------------------------------------------

INSERT IGNORE INTO tcmodule_custom_mod_providers (id, name, typeName, configurationModuleId, configurationId, view,
                                                  app_data)
VALUES (5, 'Nexus Mods', 'TCAdminCustomMods.Providers.NexusModsProvider, TCAdminCustomMods',
        'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0', 5, 'NexusModsProvider', '');

# ---------------------------------------------------------------------------------------------------------------------

DELETE FROM tc_page_icons WHERE icon_id=4895 AND module_id='d3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9';
INSERT INTO tc_page_icons (icon_id, module_id, page_id, linked_page_id, linked_page_module_id, display_name,
                           description, icon, url, display_sql, user_type, selected_user_type, view_order,
                           enabled, icon_manager, is_postback, postback_class, new_page, category_id)
VALUES (4895, 'd3b2aa93-7e2b-4e0d-8080-67d14b2fa8a9', 23, 1, 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0',
        'Nexus Mods', 'Manage Nexus Mods for $[Service.ConnectionInfo]', 'MenuIcons/GameHosting/Mods.png',
        '/Service/CustomMods/$[Service.ServiceId]?mod_provider=5', null, 0, null, 350, 1,
        'TCAdminCustomMods.IconManager, TCAdminCustomMods', null, null, null, null);
