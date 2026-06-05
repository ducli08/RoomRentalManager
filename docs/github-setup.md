# Push monorepo to GitHub

Run these steps once after creating the GitHub repository.

## 1. Create repository on GitHub

Create an empty repository: https://github.com/new

- Name: `RoomRentalManager`
- Owner: `ducli08`
- Do **not** initialize with README (monorepo already has one)

## 2. Push

```powershell
cd E:\Working\RoomRentalManager
git remote add origin https://github.com/ducli08/RoomRentalManager.git   # skip if already added
git push -u origin main
```

## 3. Archive legacy repositories

On GitHub, for each repo:

- [RoomRentalManagerServer](https://github.com/ducli08/RoomRentalManagerServer) → Settings → Archive
- [RoomRentalManagerClient](https://github.com/ducli08/RoomRentalManagerClient) → Settings → Archive

README redirect notices have been pushed to both legacy repos.
