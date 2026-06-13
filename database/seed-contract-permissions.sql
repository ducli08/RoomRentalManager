-- Seed module Contract: tạo permission MỚI cho từng module (Thêm / Sửa / Xóa)
--
-- permissions          → mỗi module có bộ action riêng (row mới, cùng tên Thêm/Sửa/Xóa)
-- role                 → module/màn hình: Contract, RoomRental, User, ...
-- rolePermission       → module ↔ permissionId của module đó
-- roleGroup            → Admin, Manager, Viewer, ...
-- roleGroupPermission  → roleGroup ↔ permissionId (chỉ id thuộc module được tick)
--
-- Login: {role.name}.{permission.name}  →  Contract.Thêm

-- Đồng bộ sequence (tránh lỗi PK trùng khi seed/import tay làm lệch nextval)
SELECT setval(pg_get_serial_sequence('role', 'id'), COALESCE((SELECT MAX(id) FROM role), 0));
SELECT setval(pg_get_serial_sequence('permissions', 'id'), COALESCE((SELECT MAX(id) FROM permissions), 0));
SELECT setval(pg_get_serial_sequence('"rolePermission"', 'id'), COALESCE((SELECT MAX(id) FROM "rolePermission"), 0));
SELECT setval(pg_get_serial_sequence('"roleGroupPermission"', 'id'), COALESCE((SELECT MAX(id) FROM "roleGroupPermission"), 0));

DO $$
DECLARE
    contract_role_id bigint;
    admin_group_id bigint;
    perm_name text;
    perm_id bigint;
    perm_names text[] := ARRAY['Thêm', 'Sửa', 'Xóa'];
BEGIN
    -- 1. Role (module) Contract
    IF NOT EXISTS (SELECT 1 FROM role WHERE name = 'Contract') THEN
        INSERT INTO role (name, active, "createdAt", "updatedAt", "creatorUser", "lastUpdateUser")
        VALUES ('Contract', TRUE, NOW(), NOW(), 'system', 'system');
    END IF;

    SELECT id INTO contract_role_id FROM role WHERE name = 'Contract';

    -- 2. Permission mới + rolePermission (bỏ qua nếu Contract đã có đủ 3 action)
    IF NOT EXISTS (
        SELECT 1 FROM "rolePermission" rp
        INNER JOIN permissions p ON p.id = rp."permissionId"
        WHERE rp."roleId" = contract_role_id AND p.name = 'Thêm'
    ) THEN
        FOREACH perm_name IN ARRAY perm_names
        LOOP
            INSERT INTO permissions (name, description, "createdAt", "creatorUser", "lastUpdateUser")
            VALUES (perm_name, 'Contract - ' || perm_name, NOW(), 'system', 'system')
            RETURNING id INTO perm_id;

            INSERT INTO "rolePermission" ("roleId", "permissionId", "createdAt", "createdBy")
            VALUES (contract_role_id, perm_id, NOW(), 'system');
        END LOOP;
    END IF;

    -- 3. Gán permission Contract cho nhóm Admin (bỏ block này nếu muốn tick tay trên UI)
    SELECT id INTO admin_group_id FROM "roleGroup" WHERE name = 'Admin';

    IF admin_group_id IS NOT NULL THEN
        INSERT INTO "roleGroupPermission" ("roleGroupId", "permissionId", "createdAt", "createdBy")
        SELECT admin_group_id, rp."permissionId", NOW(), 'system'
        FROM "rolePermission" rp
        WHERE rp."roleId" = contract_role_id
          AND NOT EXISTS (
            SELECT 1 FROM "roleGroupPermission" rgp
            WHERE rgp."roleGroupId" = admin_group_id
              AND rgp."permissionId" = rp."permissionId"
          );
    END IF;
END $$;

-- Kiểm tra permission mới của Contract
SELECT
    r.name AS module,
    p.id   AS permission_id,
    p.name AS action,
    p.description
FROM role r
INNER JOIN "rolePermission" rp ON rp."roleId" = r.id
INNER JOIN permissions p ON p.id = rp."permissionId"
WHERE r.name = 'Contract'
ORDER BY p.name;

-- Kiểm tra Admin đã được gán Contract.*
SELECT
    rg.name AS role_group,
    r.name  AS module,
    p.name  AS action,
    r.name || '.' || p.name AS permission_string
FROM "roleGroup" rg
INNER JOIN "roleGroupPermission" rgp ON rgp."roleGroupId" = rg.id
INNER JOIN permissions p ON p.id = rgp."permissionId"
INNER JOIN "rolePermission" rp ON rp."permissionId" = p.id
INNER JOIN role r ON r.id = rp."roleId"
WHERE rg.name = 'Admin' AND r.name = 'Contract'
ORDER BY p.name;
