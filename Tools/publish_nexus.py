import base64
import hashlib
import html
import json
import os
from pathlib import Path
import re
import sys
import time
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Nexus"
RELEASES = ROOT / "Releases"
RECEIPT = OUT / "PUBLICATION.json"

PAGE = 20
MOD_ID = "46785078755348"
VERSION = "1.0.0"
ARCHIVE_NAME = f"KingdomInc-HDSnapshot-{VERSION}.zip"
ARCHIVE_PATH = RELEASES / ARCHIVE_NAME

KEY = os.environ.get("NEXUSMODS_API_KEY")
if not KEY:
    raise RuntimeError("NEXUSMODS_API_KEY environment variable is missing.")

HEADERS = {
    "apikey": KEY,
    "User-Agent": "KingdomInc-HDSnapshot-Release/1.0.0",
    "Accept": "application/json",
    "Content-Type": "application/json",
}


def api(path, method="GET", body=None):
    req = urllib.request.Request(
        "https://api.nexusmods.com/v3" + path,
        method=method,
        headers=HEADERS,
        data=json.dumps(body).encode() if body is not None else None,
    )
    try:
        with urllib.request.urlopen(req, timeout=45) as res:
            raw = res.read()
            return json.loads(raw) if raw else {"status": res.status}
    except urllib.error.HTTPError as ex:
        raise RuntimeError(
            f"{method} {path}: HTTP {ex.code}: {ex.read().decode()}"
        ) from None


def save_json(path, data):
    path.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")


def norm(text):
    return re.sub(r"<br\s*/?>", "", html.unescape(text or "")).replace("\r\n", "\n").strip()


def get_mod_page():
    res = api(f"/games/kingdominc/mods/{PAGE}")["data"]
    assert str(res["game_scoped_id"]) == str(PAGE)
    return res


def main():
    print(f"Resolving Nexus Mod {PAGE}...")
    p = get_mod_page()
    print(f"Mod ID: {p['id']}, Name: {p['name']}, Status: {p['status']}")

    record = {
        "page": PAGE,
        "modId": p["id"],
        "name": p["name"],
        "version": VERSION,
        "archive": str(ARCHIVE_PATH),
    }

    # 1. Update Description
    print("Checking listing description...")
    desc_path = OUT / "DESCRIPTION.bbcode"
    if not desc_path.exists():
        desc_path = OUT / "NEXUS_DESCRIPTION.bbcode"
    target_desc = desc_path.read_text(encoding="utf-8").strip()

    if norm(p.get("description", "")) != norm(target_desc):
        print("Updating mod description on Nexus...")
        api(f"/mods/{p['id']}", "PATCH", {"description": target_desc})
        after = get_mod_page()
        record["descriptionUpdated"] = True
        print("Description updated successfully.")
    else:
        print("Description is already current.")
        record["descriptionUpdated"] = False

    # 2. Check existing files
    print("Checking existing files on mod...")
    existing = api(f"/mods/{p['id']}/files")
    files = existing.get("data", {}).get("mod_files", [])

    if files:
        print(f"Found {len(files)} existing file(s) on mod page:")
        for f in files:
            print(f"  - {f.get('name')} (id: {f.get('id')})")
        chain_id = files[0]["id"]
        record["fileChainId"] = chain_id
    else:
        # 3. Upload release archive
        assert ARCHIVE_PATH.exists(), f"Archive not found: {ARCHIVE_PATH}"
        payload = ARCHIVE_PATH.read_bytes()
        sha256 = hashlib.sha256(payload).hexdigest()
        md5 = hashlib.md5(payload).digest()

        record["sha256"] = sha256
        record["bytes"] = len(payload)

        print(f"Requesting upload ticket for {ARCHIVE_NAME} ({len(payload)} bytes)...")
        upload_ticket = api(
            "/uploads",
            "POST",
            {
                "filename": ARCHIVE_NAME,
                "size_bytes": len(payload),
                "md5": md5.hex(),
            },
        )["data"]

        upload_id = upload_ticket["id"]
        presigned_url = upload_ticket["presigned_url"]
        record["uploadId"] = upload_id

        print("Uploading binary to presigned storage URL...")
        put_req = urllib.request.Request(
            presigned_url,
            method="PUT",
            data=payload,
            headers={
                "Content-Type": "application/octet-stream",
                "Content-Disposition": f'attachment; filename="{ARCHIVE_NAME}"',
                "Content-MD5": base64.b64encode(md5).decode(),
            },
        )
        with urllib.request.urlopen(put_req, timeout=90) as put_res:
            assert put_res.status in (200, 201, 204)
        print("Binary upload complete.")

        print("Finalising upload...")
        api(f"/uploads/{upload_id}/finalise", "POST")

        print("Waiting for upload processing...")
        for attempt in range(20):
            state = api(f"/uploads/{upload_id}")["data"]["state"]
            print(f"  Upload state: {state} (poll {attempt + 1})")
            record["uploadState"] = state
            if state == "available":
                break
            if state in ("failed", "rejected", "error"):
                raise RuntimeError(f"Nexus upload processing failed with state: {state}")
            time.sleep(2)
        else:
            raise RuntimeError("Upload processing timed out.")

        file_desc = (
            "High-definition box selection and panoramic offscreen screenshot capture for Kingdom Inc. workshop flows.\n"
            "Emulates 1:1 close-up zoom and 2x Ultra-HD supersampling with crisp font filtering and smooth pipe rendering.\n"
            "Direct export to Screenshots/ folder and Windows clipboard bitmap copy.\n"
            "Requires BepInEx 5 (Windows x64 Mono) and optionally Foundation 0.5.0+."
        )

        print("Creating new mod file...")
        create_res = api(
            "/mod-files",
            "POST",
            {
                "upload_id": upload_id,
                "mod_id": p["id"],
                "name": f"HD Snapshot {VERSION}",
                "version": VERSION,
                "file_category": "main",
                "description": file_desc,
                "update_mod_version": True,
                "primary_mod_manager_download": False,
                "allow_mod_manager_download": False,
                "show_requirements_pop_up": True,
            },
        )
        record["createResponse"] = create_res
        print("Mod file successfully created!")

        # Refresh files list
        existing = api(f"/mods/{p['id']}/files")
        files = existing.get("data", {}).get("mod_files", [])
        assert files, "No files found after creation!"
        chain_id = files[0]["id"]
        record["fileChainId"] = chain_id

    # 4. Post Changelog
    print("Checking changelog...")
    try:
        changelog_text = (
            "Initial release 1.0.0.\n"
            "- Box selection tool with live pixel dimension readout.\n"
            "- Full-fidelity 1:1 and 2x offscreen tiled capture engine.\n"
            "- Automated PNG export to Screenshots/ folder.\n"
            "- Windows clipboard bitmap and path copy.\n"
            "- Workshop Grid, Transparent PNG, and Dark Slate background options.\n"
            "- Foundation ModConfig and hotbar integration."
        )
        cl_res = api(
            f"/mods/{p['id']}/changelogs",
            "POST",
            {"version": VERSION, "changelog": changelog_text},
        )
        record["changelogResponse"] = cl_res
        print("Changelog posted.")
    except Exception as ex:
        print(f"Changelog note: {ex}")

    # 5. Verify file chain versions and manifest
    print(f"Verifying versions for chain {chain_id}...")
    versions_res = api(f"/mod-files/{chain_id}/versions")
    versions = versions_res.get("data", {}).get("versions", [])
    matching = [v for v in versions if v["version"] == VERSION]
    assert matching, f"No matching version {VERSION} found on chain {chain_id}!"

    latest_ver = matching[0]
    record["fileVersionId"] = latest_ver["id"]
    record["gameScopedFileVersionId"] = latest_ver.get("game_scoped_id")
    print(f"Verified version: {latest_ver['version']} (id: {latest_ver['id']})")

    if "manifest_url" in latest_ver and latest_ver["manifest_url"]:
        print("Verifying manifest digest...")
        manifest_req = urllib.request.Request(
            latest_ver["manifest_url"], headers={"User-Agent": HEADERS["User-Agent"]}
        )
        for attempt in range(15):
            try:
                with urllib.request.urlopen(manifest_req, timeout=45) as m_res:
                    manifest = json.loads(m_res.read().decode("utf-8"))
                    record["manifest"] = manifest
                    archive_sha = manifest.get("archive", {}).get("hashes", {}).get("SHA256")
                    print(f"Nexus manifest SHA256: {archive_sha}")
                    if "sha256" in record:
                        assert archive_sha.lower() == record["sha256"].lower(), "Manifest digest mismatch!"
                    record["manifestVerified"] = True
                    break
            except urllib.error.HTTPError as m_err:
                if m_err.code == 404 and attempt < 14:
                    time.sleep(2)
                    continue
                raise

    save_json(RECEIPT, record)
    print(f"Publication receipt saved to: {RECEIPT}")
    print("\nSUCCESS: HD Snapshot 1.0.0 published to Nexus Mods page 20!")


if __name__ == "__main__":
    main()
