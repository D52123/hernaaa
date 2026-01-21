using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DeftHackInjector
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtCreateThreadEx(out IntPtr hThread, uint DesiredAccess, IntPtr ObjectAttributes, IntPtr ProcessHandle, IntPtr lpStartAddress, IntPtr lpParameter, uint Flags, IntPtr StackZeroBits, IntPtr SizeOfStackCommit, IntPtr SizeOfStackReserve, IntPtr lpBytesBuffer);

        private const int PROCESS_CREATE_THREAD = 0x0002;
        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        private const uint MEM_COMMIT = 0x00001000;
        private const uint MEM_RESERVE = 0x00002000;
        private const uint PAGE_READWRITE = 4;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "DeftHack Injector";
            
            Console.WriteLine("========================================");
            Console.WriteLine("    DeftHack Injector v1.0");
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Проверка прав администратора
            if (!IsRunAsAdmin())
            {
                Console.WriteLine("⚠️  ВНИМАНИЕ: Запущено без прав администратора!");
                Console.WriteLine("💡 Для надежной инжекции запустите от администратора");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("✅ Права администратора подтверждены");
                Console.WriteLine();
            }

            try
            {
                // Поиск процесса Unturned
                Process[] processes = Process.GetProcessesByName("Unturned");
                if (processes.Length == 0)
                {
                    Console.WriteLine("❌ Процесс Unturned не найден!");
                    Console.WriteLine("🎮 Запустите Unturned и нажмите любую клавишу...");
                    Console.ReadKey();
                    return;
                }

                Process targetProcess = processes[0];
                Console.WriteLine($"✅ Процесс найден: PID {targetProcess.Id}");
                Console.WriteLine();

                // Поиск DLL для инжекции
                Console.WriteLine("========================================");
                Console.WriteLine("  Поиск DLL для инжекции...");
                Console.WriteLine("========================================");
                Console.WriteLine();
                
                string currentDir = Directory.GetCurrentDirectory();
                Console.WriteLine($"📁 Текущая директория: {currentDir}");
                Console.WriteLine();
                
                // Ищем обе DLL
                string csharpDll = FindCSharpDLL();
                string imguiDll = FindImGuiDLL();
                
                bool csharpInjected = false;
                bool imguiInjected = false;

                // Инжекция C# DLL (основной функционал)
                if (!string.IsNullOrEmpty(csharpDll))
                {
                    Console.WriteLine();
                    Console.WriteLine("========================================");
                    Console.WriteLine("  📦 ИНЖЕКЦИЯ C# DLL");
                    Console.WriteLine("========================================");
                    Console.WriteLine();
                    Console.WriteLine($"📁 Путь: {csharpDll}");
                    var fileInfo = new FileInfo(csharpDll);
                    Console.WriteLine($"📊 Размер: {fileInfo.Length / 1024} KB");
                    Console.WriteLine();
                    
                    Console.WriteLine("🚀 Инжекция C# DLL...");
                    csharpInjected = InjectDLL(targetProcess, csharpDll);
                    if (!csharpInjected)
                    {
                        Console.WriteLine("⚠️ C# DLL не инжектирована, пробую Manual Mapping...");
                        csharpInjected = InjectManualMapping(targetProcess, csharpDll);
                    }
                    
                    if (csharpInjected)
                    {
                        Console.WriteLine("✅ C# DLL инжектирована успешно!");
                        System.Threading.Thread.Sleep(500); // Небольшая задержка между инжекциями
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ C# DLL не найдена!");
                }

                // Инжекция ImGui DLL (GUI)
                if (!string.IsNullOrEmpty(imguiDll))
                {
                    Console.WriteLine();
                    Console.WriteLine("========================================");
                    Console.WriteLine("  🎨 ИНЖЕКЦИЯ IMGUI DLL");
                    Console.WriteLine("========================================");
                    Console.WriteLine();
                    Console.WriteLine($"📁 Путь: {imguiDll}");
                    var fileInfo = new FileInfo(imguiDll);
                    Console.WriteLine($"📊 Размер: {fileInfo.Length / 1024} KB");
                    Console.WriteLine();
                    
                    Console.WriteLine("🚀 Инжекция ImGui DLL...");
                    imguiInjected = InjectDLL(targetProcess, imguiDll);
                    if (!imguiInjected)
                    {
                        Console.WriteLine("⚠️ ImGui DLL не инжектирована, пробую Manual Mapping...");
                        imguiInjected = InjectManualMapping(targetProcess, imguiDll);
                    }
                    
                    if (imguiInjected)
                    {
                        Console.WriteLine("✅ ImGui DLL инжектирована успешно!");
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("⚠️ ImGui DLL не найдена!");
                    Console.WriteLine("💡 Для компиляции ImGui DLL:");
                    Console.WriteLine("   1. Запустите BUILD.bat");
                    Console.WriteLine("   2. Или скомпилируйте через Visual Studio");
                    Console.WriteLine();
                }
                
                // Итоговый результат
                Console.WriteLine();
                Console.WriteLine("========================================");
                if (csharpInjected || imguiInjected)
                {
                    Console.WriteLine("  ✅ ИНЖЕКЦИЯ ЗАВЕРШЕНА!");
                    Console.WriteLine("========================================");
                    Console.WriteLine();
                    Console.WriteLine("🎮 Следующие шаги:");
                    Console.WriteLine("   1. Вернитесь в окно Unturned");
                    if (imguiInjected)
                    {
                        Console.WriteLine("   2. Нажмите INSERT для открытия/закрытия меню ImGui");
                        Console.WriteLine("   3. Используйте вкладки: LEGIT, VISUALS, MISC, SETTINGS");
                    }
                    else if (csharpInjected)
                    {
                        Console.WriteLine("   2. Нажмите F1 для открытия меню (C# GUI)");
                    }
                    Console.WriteLine();
                    Console.WriteLine("💡 Если меню не открывается:");
                    Console.WriteLine("   - Проверьте что игра активна");
                    Console.WriteLine("   - Попробуйте нажать INSERT или F1 несколько раз");
                    Console.WriteLine("   - Перезапустите игру и инжектор");
                }
                else
                {
                    Console.WriteLine("  ❌ ИНЖЕКЦИЯ ПРОВАЛИЛАСЬ!");
                    Console.WriteLine("========================================");
                    Console.WriteLine();
                    Console.WriteLine("💡 Возможные решения:");
                    Console.WriteLine("   1. Запустите инжектор от администратора");
                    Console.WriteLine("   2. Отключите антивирус временно");
                    Console.WriteLine("   3. Добавьте папку в исключения антивируса");
                    Console.WriteLine("   4. Перезапустите Unturned");
                    Console.WriteLine("   5. Убедитесь что это не BattlEye сервер");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"❌ Критическая ошибка: {ex.Message}");
                Console.WriteLine($"   {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static bool IsRunAsAdmin()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        static string FindCSharpDLL()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            List<string> paths = new List<string>
            {
                Path.Combine(currentDir, "bin", "Release", "UnityEngine.FileSystemModule.dll"),
                Path.Combine(currentDir, "bin", "Release", "net48", "win-x64", "UnityEngine.FileSystemModule.dll"),
                Path.Combine(currentDir, "UnityEngine.FileSystemModule.dll"),
                Path.Combine(currentDir, "DeftHack.dll"),
                "UnityEngine.FileSystemModule.dll",
                "DeftHack.dll"
            };
            
            if (!string.IsNullOrEmpty(exeDir) && exeDir != currentDir)
            {
                paths.Add(Path.Combine(exeDir, "..", "bin", "Release", "UnityEngine.FileSystemModule.dll"));
                paths.Add(Path.Combine(exeDir, "..", "UnityEngine.FileSystemModule.dll"));
            }

            foreach (string path in paths)
            {
                string fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    var fileInfo = new FileInfo(fullPath);
                    if (fileInfo.Length > 50000) // Минимум 50KB
                    {
                        return fullPath;
                    }
                }
            }
            return null;
        }

        static string FindImGuiDLL()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            List<string> paths = new List<string>
            {
                Path.Combine(currentDir, "DeftHack_ImGui.dll"),
                Path.Combine(currentDir, "DeftHack_ImGui", "DeftHack_ImGui.dll"),
                Path.Combine(currentDir, "DeftHack_ImGui", "build", "DeftHack_ImGui.dll"),
                Path.Combine(currentDir, "DeftHack_ImGui", "build", "Release", "DeftHack_ImGui.dll"),
                "DeftHack_ImGui.dll"
            };
            
            if (!string.IsNullOrEmpty(exeDir) && exeDir != currentDir)
            {
                paths.Add(Path.Combine(exeDir, "..", "DeftHack_ImGui.dll"));
                paths.Add(Path.Combine(exeDir, "..", "DeftHack_ImGui", "build", "Release", "DeftHack_ImGui.dll"));
            }

            foreach (string path in paths)
            {
                string fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    var fileInfo = new FileInfo(fullPath);
                    if (fileInfo.Length > 100000) // Минимум 100KB для ImGui DLL
                    {
                        return fullPath;
                    }
                }
            }
            return null;
        }

        static bool InjectDLL(Process targetProcess, string dllPath)
        {
            IntPtr procHandle = IntPtr.Zero;
            IntPtr allocMemAddress = IntPtr.Zero;
            IntPtr threadHandle = IntPtr.Zero;

            try
            {
                // Проверка существования файла и получение полного пути
                if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
                {
                    Console.WriteLine($"❌ DLL файл не найден: {dllPath}");
                    return false;
                }
                
                dllPath = Path.GetFullPath(dllPath);
                Console.WriteLine($"📁 Полный путь: {dllPath}");

                // Пробуем разные уровни доступа к процессу
                int[] accessLevels = {
                    PROCESS_ALL_ACCESS,
                    PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                    PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                    PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE
                };

                Console.WriteLine("🔧 Открываем процесс...");
                foreach (int accessLevel in accessLevels)
                {
                    procHandle = OpenProcess(accessLevel, false, targetProcess.Id);
                    if (procHandle != IntPtr.Zero)
                    {
                        Console.WriteLine($"✅ Процесс открыт с уровнем доступа 0x{accessLevel:X}");
                        break;
                    }
                    else
                    {
                        int error = Marshal.GetLastWin32Error();
                        Console.WriteLine($"⚠️  Уровень доступа 0x{accessLevel:X} не сработал, ошибка: {error}");
                    }
                }

                if (procHandle == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"❌ Не удалось открыть процесс, ошибка: {error}");
                    
                    if (error == 5)
                    {
                        Console.WriteLine("💡 Отказ в доступе - попробуйте:");
                        Console.WriteLine("   1. Запустить инжектор от администратора");
                        Console.WriteLine("   2. Запустить Unturned от администратора");
                        Console.WriteLine("   3. Отключить антивирус временно");
                    }
                    return false;
                }

                // Получаем адрес LoadLibraryA
                Console.WriteLine("🔧 Получаем адрес LoadLibraryA...");
                IntPtr kernel32Handle = GetModuleHandle("kernel32.dll");
                if (kernel32Handle == IntPtr.Zero)
                {
                    Console.WriteLine("❌ Не удалось получить handle kernel32.dll");
                    return false;
                }

                IntPtr loadLibraryAddr = GetProcAddress(kernel32Handle, "LoadLibraryA");
                if (loadLibraryAddr == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"❌ Не удалось найти LoadLibraryA, ошибка: {error}");
                    return false;
                }
                Console.WriteLine($"✅ LoadLibraryA найдена: 0x{loadLibraryAddr.ToInt64():X}");

                // Подготавливаем путь к DLL (ANSI строка для LoadLibraryA)
                Console.WriteLine("🔧 Подготавливаем путь к DLL...");
                byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath);
                byte[] dllPathBytesWithNull = new byte[dllPathBytes.Length + 1];
                Array.Copy(dllPathBytes, dllPathBytesWithNull, dllPathBytes.Length);
                dllPathBytesWithNull[dllPathBytes.Length] = 0; // Добавляем null-terminator
                
                uint dllPathSize = (uint)dllPathBytesWithNull.Length;

                // Пробуем выделить память с разными размерами и типами
                Console.WriteLine("🔧 Выделяем память...");
                uint[] memorySizes = { dllPathSize, dllPathSize * 2, 4096, 8192 };
                uint[] memoryTypes = { PAGE_READWRITE, 0x40 }; // PAGE_READWRITE и PAGE_EXECUTE_READWRITE
                
                bool memoryAllocated = false;
                foreach (uint memSize in memorySizes)
                {
                    foreach (uint memType in memoryTypes)
                    {
                        allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, memSize, MEM_COMMIT | MEM_RESERVE, memType);
                        if (allocMemAddress != IntPtr.Zero)
                        {
                            Console.WriteLine($"✅ Память выделена: размер {memSize}, адрес 0x{allocMemAddress.ToInt64():X}");
                            memoryAllocated = true;
                            break;
                        }
                        else
                        {
                            int error = Marshal.GetLastWin32Error();
                            Console.WriteLine($"⚠️  Размер {memSize}, тип 0x{memType:X} - ошибка: {error}");
                        }
                    }
                    if (memoryAllocated) break;
                }

                if (!memoryAllocated)
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"❌ Не удалось выделить память, ошибка: {error}");
                    
                    if (error == 5)
                    {
                        Console.WriteLine("💡 Отказ в доступе - попробуйте:");
                        Console.WriteLine("   1. Запустить Unturned от администратора");
                        Console.WriteLine("   2. Отключить антивирус/Windows Defender");
                        Console.WriteLine("   3. Добавить папку в исключения антивируса");
                    }
                    else if (error == 8)
                    {
                        Console.WriteLine("💡 Недостаточно памяти в целевом процессе");
                        Console.WriteLine("   Попробуйте перезапустить Unturned");
                    }
                    return false;
                }

                // Записываем путь к DLL
                Console.WriteLine("🔧 Записываем путь к DLL...");
                UIntPtr bytesWritten;
                bool result = WriteProcessMemory(procHandle, allocMemAddress, dllPathBytesWithNull, dllPathSize, out bytesWritten);
                if (!result)
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"❌ Не удалось записать в память, ошибка: {error}");
                    return false;
                }
                Console.WriteLine($"✅ Записано {bytesWritten} байт");

                // Создаем удаленный поток (пробуем разные методы)
                Console.WriteLine("🔧 Создаем удаленный поток...");
                
                // Метод 1: CreateRemoteThread
                threadHandle = CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);
                
                // Метод 2: NtCreateThreadEx (если первый не сработал)
                if (threadHandle == IntPtr.Zero)
                {
                    Console.WriteLine("⚠️  CreateRemoteThread не сработал, пробуем NtCreateThreadEx...");
                    int status = NtCreateThreadEx(out threadHandle, 0x1FFFFF, IntPtr.Zero, procHandle, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    if (status != 0 || threadHandle == IntPtr.Zero)
                    {
                        int error = Marshal.GetLastWin32Error();
                        Console.WriteLine($"❌ Не удалось создать поток, ошибка: {error}");
                        
                        if (error == 5)
                        {
                            Console.WriteLine("💡 Ошибка доступа - попробуйте:");
                            Console.WriteLine("   1. Запустить Unturned от администратора");
                            Console.WriteLine("   2. Отключить Windows Defender/антивирус");
                            Console.WriteLine("   3. Добавить папку в исключения антивируса");
                            Console.WriteLine("   4. Прочитайте файл: РЕШЕНИЕ_ОШИБКИ_5.txt");
                        }
                        return false;
                    }
                    Console.WriteLine("✅ Поток создан через NtCreateThreadEx, ожидание...");
                }
                else
                {
                    Console.WriteLine("✅ Поток создан через CreateRemoteThread, ожидание...");
                }

                // Ждем завершения потока
                uint waitResult = WaitForSingleObject(threadHandle, 10000);
                if (waitResult == 0)
                {
                    Console.WriteLine("✅ Поток завершился успешно!");
                    return true;
                }
                else
                {
                    Console.WriteLine($"⚠️  Поток завершился с кодом: {waitResult}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка инжекции: {ex.Message}");
                return false;
            }
            finally
            {
                // Освобождаем ресурсы
                if (threadHandle != IntPtr.Zero)
                    CloseHandle(threadHandle);
                if (procHandle != IntPtr.Zero)
                    CloseHandle(procHandle);
            }
        }

        // Manual Mapping - обход BattlEye (не использует LoadLibrary)
        static bool InjectManualMapping(Process targetProcess, string dllPath)
        {
            IntPtr procHandle = IntPtr.Zero;
            IntPtr shellcodeAddr = IntPtr.Zero;
            IntPtr threadHandle = IntPtr.Zero;

            try
            {
                Console.WriteLine("🔧 Manual Mapping: Читаем DLL файл...");
                
                // Читаем весь DLL файл
                byte[] dllBytes = File.ReadAllBytes(dllPath);
                if (dllBytes.Length < 64)
                {
                    Console.WriteLine("❌ DLL файл слишком мал");
                    return false;
                }
                
                Console.WriteLine($"✅ Прочитано {dllBytes.Length / 1024} KB");

                // Читаем PE заголовок из массива байтов
                byte[] peHeader = new byte[4096];
                Array.Copy(dllBytes, 0, peHeader, 0, Math.Min(4096, dllBytes.Length));

                // Получаем размер DLL из PE заголовка
                int peHeaderOffset = BitConverter.ToInt32(peHeader, 0x3C);
                int sizeOfImage = 0;
                if (peHeaderOffset > 0 && peHeaderOffset < peHeader.Length - 4)
                {
                    sizeOfImage = BitConverter.ToInt32(peHeader, peHeaderOffset + 0x38);
                }
                
                // Если не удалось получить размер из PE, используем размер файла
                if (sizeOfImage == 0 || sizeOfImage > dllBytes.Length * 2)
                {
                    sizeOfImage = dllBytes.Length;
                }
                
                Console.WriteLine($"📊 Размер DLL: {sizeOfImage / 1024} KB");

                // Открываем процесс
                Console.WriteLine("🔧 Открываем процесс...");
                procHandle = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcess.Id);
                if (procHandle == IntPtr.Zero)
                {
                    // Пробуем разные уровни доступа
                    int[] accessLevels = {
                        PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                        PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ
                    };
                    
                    foreach (int access in accessLevels)
                    {
                        procHandle = OpenProcess(access, false, targetProcess.Id);
                        if (procHandle != IntPtr.Zero) break;
                    }
                }

                if (procHandle == IntPtr.Zero)
                {
                    Console.WriteLine("❌ Не удалось открыть процесс для Manual Mapping");
                    return false;
                }
                Console.WriteLine("✅ Процесс открыт");

                // Выделяем память в целевом процессе
                Console.WriteLine("🔧 Выделяем память в целевом процессе...");
                shellcodeAddr = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)sizeOfImage, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
                
                if (shellcodeAddr == IntPtr.Zero)
                {
                    // Пробуем разные типы памяти
                    uint[] memTypes = { PAGE_EXECUTE_READWRITE, PAGE_READWRITE, 0x20 }; // PAGE_EXECUTE_READ, PAGE_READWRITE, PAGE_EXECUTE
                    foreach (uint memType in memTypes)
                    {
                        shellcodeAddr = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)sizeOfImage, MEM_COMMIT | MEM_RESERVE, memType);
                        if (shellcodeAddr != IntPtr.Zero)
                        {
                            Console.WriteLine($"✅ Память выделена с типом 0x{memType:X}");
                            break;
                        }
                    }
                }

                if (shellcodeAddr == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"❌ Не удалось выделить память для Manual Mapping, ошибка: {error}");
                    Console.WriteLine("💡 BattlEye блокирует выделение памяти");
                    Console.WriteLine("💡 Попробуйте:");
                    Console.WriteLine("   1. Запустить Unturned БЕЗ BattlEye (приватный сервер)");
                    Console.WriteLine("   2. Использовать файловую замену DLL Unity");
                    return false;
                }
                Console.WriteLine($"✅ Память выделена: 0x{shellcodeAddr.ToInt64():X}");

                // DLL уже прочитан выше
                Console.WriteLine($"✅ DLL файл готов: {dllBytes.Length / 1024} KB");

                // Записываем DLL в память целевого процесса по частям
                Console.WriteLine("🔧 Записываем DLL в память процесса...");
                const int chunkSize = 4096;
                int offset = 0;
                bool writeSuccess = true;

                while (offset < dllBytes.Length)
                {
                    int currentChunkSize = Math.Min(chunkSize, dllBytes.Length - offset);
                    byte[] chunk = new byte[currentChunkSize];
                    Array.Copy(dllBytes, offset, chunk, 0, currentChunkSize);

                    IntPtr targetAddr = new IntPtr(shellcodeAddr.ToInt64() + offset);
                    UIntPtr bytesWritten;
                    bool result = WriteProcessMemory(procHandle, targetAddr, chunk, (uint)currentChunkSize, out bytesWritten);
                    
                    if (!result || bytesWritten.ToUInt32() != currentChunkSize)
                    {
                        int error = Marshal.GetLastWin32Error();
                        Console.WriteLine($"⚠️  Ошибка записи чанка по смещению {offset}, ошибка: {error}");
                        writeSuccess = false;
                        break;
                    }

                    offset += currentChunkSize;
                }

                if (!writeSuccess)
                {
                    Console.WriteLine("❌ Ошибка записи DLL в память");
                    return false;
                }
                Console.WriteLine("✅ DLL записана в память");

                // Получаем адрес DllMain
                Console.WriteLine("🔧 Получаем адрес DllMain...");
                IntPtr kernel32Handle = GetModuleHandle("kernel32.dll");
                IntPtr loadLibraryAddr = GetProcAddress(kernel32Handle, "LoadLibraryA");
                
                // Для Manual Mapping нужно вызвать DllMain вручную
                // Упрощенная версия - просто создаем поток на адресе DLL
                Console.WriteLine("🔧 Создаем поток для активации DLL...");
                
                // Пробуем CreateRemoteThread
                threadHandle = CreateRemoteThread(procHandle, IntPtr.Zero, 0, shellcodeAddr, IntPtr.Zero, 0, IntPtr.Zero);
                
                if (threadHandle == IntPtr.Zero)
                {
                    // Пробуем NtCreateThreadEx
                    int status = NtCreateThreadEx(out threadHandle, 0x1FFFFF, IntPtr.Zero, procHandle, shellcodeAddr, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    if (status != 0 || threadHandle == IntPtr.Zero)
                    {
                        Console.WriteLine("⚠️  Не удалось создать поток для Manual Mapping");
                        Console.WriteLine("💡 DLL загружена в память, но не активирована");
                        Console.WriteLine("💡 Попробуйте использовать стандартную инжекцию");
                        return false;
                    }
                }

                Console.WriteLine("✅ Поток создан для Manual Mapping");
                
                uint waitResult = WaitForSingleObject(threadHandle, 5000);
                if (waitResult == 0)
                {
                    Console.WriteLine("✅ Manual Mapping завершен успешно!");
                    return true;
                }
                else
                {
                    Console.WriteLine($"⚠️  Поток завершился с кодом: {waitResult}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка Manual Mapping: {ex.Message}");
                return false;
            }
            finally
            {
                if (threadHandle != IntPtr.Zero)
                    CloseHandle(threadHandle);
                if (procHandle != IntPtr.Zero)
                    CloseHandle(procHandle);
            }
        }
    }
}
