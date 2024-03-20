using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Chronofoil;

public class Logging
{
	private static IPluginLog _log = null!;

	public static void Initialize(IPluginLog log)
	{
		_log = log;
	}
	
	public static void Fatal(string messageTemplate, params object[] values) => _log.Fatal(messageTemplate, values);
	public static void Fatal(Exception? exception, string messageTemplate, params object[] values) => _log.Fatal(exception, messageTemplate, values);
	public static void Error(string messageTemplate, params object[] values) => _log.Error(messageTemplate, values);
	public static void Error(Exception? exception, string messageTemplate, params object[] values) => _log.Error(exception, messageTemplate, values);
	public static void Warning(string messageTemplate, params object[] values) => _log.Warning(messageTemplate, values);
	public static void Warning(Exception? exception, string messageTemplate, params object[] values) => _log.Warning(exception, messageTemplate, values);
	public static void Information(string messageTemplate, params object[] values) => _log.Information(messageTemplate, values);
	public static void Information(Exception? exception, string messageTemplate, params object[] values) => _log.Information(exception, messageTemplate, values);
	public static void Info(string messageTemplate, params object[] values) => _log.Info(messageTemplate, values);
	public static void Info(Exception? exception, string messageTemplate, params object[] values) => _log.Info(exception, messageTemplate, values);
	public static void Debug(string messageTemplate, params object[] values) => _log.Debug(messageTemplate, values);
	public static void Debug(Exception? exception, string messageTemplate, params object[] values) => _log.Debug(exception, messageTemplate, values);
	public static void Verbose(string messageTemplate, params object[] values) => _log.Verbose(messageTemplate, values);
	public static void Verbose(Exception? exception, string messageTemplate, params object[] values) => _log.Verbose(exception, messageTemplate, values);
}