DROP TABLE tcmodule_custom_mod_providers;

DELETE FROM ar_common_configurations WHERE moduleId LIKE 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0';
DELETE FROM tc_page_icons WHERE linked_page_module_id LIKE 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0';
DELETE FROM tc_site_map WHERE module_id LIKE 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0';
DELETE FROM tc_modules WHERE module_id LIKE 'b48cfbc9-7acc-4980-89c4-2b6a1f784aa0';