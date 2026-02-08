using LernDotnet.Data;
using LernDotnet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
 
namespace LernDotnet.Controllers;
 
[ApiController]
[Route("api/notes")]
public sealed class NotesController : ControllerBase
{
    private readonly AppDbContext _db;
 
    public NotesController(AppDbContext db)
    {
        _db = db;
    }
 
    [HttpGet]
    public async Task<ActionResult<List<Note>>> GetAll(CancellationToken ct)
    {
        var notes = await _db.Notes
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);
 
        return notes;
    }
 
    [HttpGet("{id:int}", Name = "GetNoteById")]
    public async Task<ActionResult<Note>> GetById(int id, CancellationToken ct)
    {
        var note = await _db.Notes.FindAsync([id], ct);
        if (note is null)
            return NotFound();
 
        return note;
    }
 
    public sealed record CreateNoteRequest(string Title, string Content);
 
    [HttpPost]
    public async Task<ActionResult<Note>> Create(CreateNoteRequest request, CancellationToken ct)
    {
        var note = new Note
        {
            Title = request.Title,
            Content = request.Content
        };
 
        _db.Notes.Add(note);
        await _db.SaveChangesAsync(ct);
 
        return CreatedAtRoute("GetNoteById", new { id = note.Id }, note);
    }
 
    public sealed record UpdateNoteRequest(string Title, string Content);
 
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Note>> Update(int id, UpdateNoteRequest request, CancellationToken ct)
    {
        var note = await _db.Notes.FindAsync([id], ct);
        if (note is null)
            return NotFound();
 
        note.Title = request.Title;
        note.Content = request.Content;
 
        await _db.SaveChangesAsync(ct);
 
        return note;
    }
 
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var note = await _db.Notes.FindAsync([id], ct);
        if (note is null)
            return NotFound();
 
        _db.Notes.Remove(note);
        await _db.SaveChangesAsync(ct);
 
        return NoContent();
    }
}