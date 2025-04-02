using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.RegistrationManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
    public class ClassRegistrationController : Controller
    {
        private readonly AppDbContext _context;

        public ClassRegistrationController(AppDbContext context)
        {
            _context = context;
        }

        // แสดงหน้าลงทะเบียน
        public async Task<IActionResult> Register()
        {
            try
            {
                var classes = await _context.Classes
                    .OrderBy(c => c.ClassNumber)
                    .ToListAsync();

                var courses = await _context.Course
                    .OrderBy(c => c.CourseName)
                    .ToListAsync();

                var semesters = await _context.Semesters
                    .OrderByDescending(s => s.SemesterYear)
                    .ThenByDescending(s => s.SemesterNumber)
                    .ToListAsync();

                ViewBag.Classes = classes;
                ViewBag.Courses = courses;
                ViewBag.Semesters = semesters;

                return View();
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["Error"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล กรุณาลองอีกครั้ง";
                return View();
            }
        }

        // ดำเนินการลงทะเบียน
        [HttpPost]
        public async Task<IActionResult> Register(int classId, List<int> courseIds, int semesterId)
        {
            if (courseIds == null || !courseIds.Any())
            {
                TempData["Error"] = "กรุณาเลือกอย่างน้อย 1 วิชา";
                return RedirectToAction("Register");
            }

            try
            {
                // ตรวจสอบว่ามีนักเรียนในห้องหรือไม่
                var students = await _context.Students
                    .Where(s => s.ClassId == classId)
                    .Select(s => s.StudentId)
                    .ToListAsync();

                if (!students.Any())
                {
                    TempData["Error"] = "ไม่มีนักเรียนในห้องนี้";
                    return RedirectToAction("Register");
                }

                // ตรวจสอบว่านักเรียนมีการลงทะเบียนในวิชาและภาคเรียนที่เลือกไว้แล้วหรือไม่
                var existingRegistrations = await _context.Set<RegisteredCourse>()
                    .Where(r => students.Contains(r.StudentId) &&
                                courseIds.Contains(r.CourseId) &&
                                r.SemesterId == semesterId)
                    .ToListAsync();

                if (existingRegistrations.Any())
                {
                    // แยกรายการที่ซ้ำออก
                    var duplicates = new Dictionary<int, List<int>>();
                    foreach (var reg in existingRegistrations)
                    {
                        if (!duplicates.ContainsKey(reg.StudentId))
                        {
                            duplicates[reg.StudentId] = new List<int>();
                        }
                        duplicates[reg.StudentId].Add(reg.CourseId);
                    }

                    // ลงทะเบียนเฉพาะรายการที่ไม่ซ้ำ
                    int registrationCount = 0;
                    foreach (var studentId in students)
                    {
                        foreach (var courseId in courseIds)
                        {
                            if (!duplicates.ContainsKey(studentId) || !duplicates[studentId].Contains(courseId))
                            {
                                _context.Set<RegisteredCourse>().Add(new RegisteredCourse
                                {
                                    StudentId = studentId,
                                    CourseId = courseId,
                                    SemesterId = semesterId,
                                    Score = 0,
                                    Grade = 0,
                                    RegisteredDate = DateTime.Now
                                });
                                registrationCount++;
                            }
                        }
                    }

                    if (registrationCount > 0)
                    {
                        await _context.SaveChangesAsync();
                        TempData["Success"] = $"ลงทะเบียนสำเร็จ {registrationCount} รายการ (ข้ามรายการที่ซ้ำ)";
                    }
                    else
                    {
                        TempData["Error"] = "นักเรียนทุกคนได้ลงทะเบียนในวิชาที่เลือกไว้แล้ว";
                    }
                }
                else
                {
                    // ลงทะเบียนทั้งหมด
                    var registrations = new List<RegisteredCourse>();
                    foreach (var studentId in students)
                    {
                        foreach (var courseId in courseIds)
                        {
                            registrations.Add(new RegisteredCourse
                            {
                                StudentId = studentId,
                                CourseId = courseId,
                                SemesterId = semesterId,
                                Score = 0,
                                Grade = 0,
                                RegisteredDate = DateTime.Now
                            });
                        }
                    }

                    await _context.Set<RegisteredCourse>().AddRangeAsync(registrations);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"ลงทะเบียนสำเร็จ {registrations.Count} รายการ";
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["Error"] = "เกิดข้อผิดพลาดในการลงทะเบียน กรุณาลองอีกครั้ง";
            }

            return RedirectToAction("Register");
        }
    }
}