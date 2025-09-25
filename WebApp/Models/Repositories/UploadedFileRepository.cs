using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models.UploadedFileModel;

namespace WebApp.Models.Repositories;

public class UploadedFileRepository : IUploadedFileRepository
{
    private readonly AppDbContext _context;

    public UploadedFileRepository(AppDbContext context) => _context = context;
    public async Task<IEnumerable<UploadedFile>> GetAll()
    {
        return await _context.UploadedFiles.ToListAsync();
    }

    public async Task<UploadedFile> Get(int id)
    {
        var uf = await _context.UploadedFiles.FindAsync(id);
        if (uf == null)
            throw new KeyNotFoundException($"UploadedFile with id {id} not found.");
        else
            return uf;
    }

    public async Task<IEnumerable<UploadedFile>> GetUserFile(int userId)
    {
        return await _context.UploadedFiles.Where(uf => uf.UserId == userId).ToListAsync();
    }
    public async Task<string> Add(UploadedFile uploadedFile)
    {
        await _context.UploadedFiles.AddAsync(uploadedFile);
        await _context.SaveChangesAsync();
        return uploadedFile.FileName;
    }

    public async Task Add(IEnumerable<UploadedFile> uploadedFiles)
    {
        await _context.UploadedFiles.AddRangeAsync(uploadedFiles);
        await _context.SaveChangesAsync();
    }

    public async Task<string> Update(UploadedFile uploadedFile)
    {
        _context.UploadedFiles.Update(uploadedFile);
        await _context.SaveChangesAsync();
        return uploadedFile.FileName;
    }

    public async Task<string> Delete(int id)
    {
        var uf = await _context.UploadedFiles.FindAsync(id);
        if (uf != null)
        {
            _context.UploadedFiles.Remove(uf);
            await _context.SaveChangesAsync();
            return uf.FileName;
        }
        return "";
    }

    public async Task Delete(IEnumerable<UploadedFile> files)
    {
        _context.RemoveRange(files);
        await _context.SaveChangesAsync();
    }

}