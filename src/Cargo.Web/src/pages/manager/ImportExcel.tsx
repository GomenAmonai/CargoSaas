import { useState, useRef } from 'react';
import { managerApi } from '../../api/manager';
import ManagerLayout from '../../components/manager/ManagerLayout';

const ImportExcel = () => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [result, setResult] = useState<{
    successCount: number;
    failedCount: number;
    errors: string[];
  } | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Проверяем расширение
      const validExtensions = ['.xlsx', '.xls'];
      const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
      
      if (!validExtensions.includes(fileExtension)) {
        alert('Please select an Excel file (.xlsx or .xls)');
        return;
      }

      setSelectedFile(file);
      setResult(null); // Очищаем предыдущий результат
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      alert('Please select a file first');
      return;
    }

    try {
      setIsUploading(true);
      const data = await managerApi.import.uploadExcel(selectedFile);
      setResult(data);

      // Очищаем выбранный файл после успешной загрузки
      setSelectedFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    } catch (err) {
      const error = err as { response?: { data?: { message?: string } } };
      alert(error.response?.data?.message || 'Failed to upload file');
    } finally {
      setIsUploading(false);
    }
  };

  const handleClearFile = () => {
    setSelectedFile(null);
    setResult(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();

    const file = e.dataTransfer.files[0];
    if (file) {
      const validExtensions = ['.xlsx', '.xls'];
      const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
      
      if (!validExtensions.includes(fileExtension)) {
        alert('Please select an Excel file (.xlsx or .xls)');
        return;
      }

      setSelectedFile(file);
      setResult(null);
    }
  };

  return (
    <ManagerLayout>
      <div className="p-8">
        {/* Header */}
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            Import Tracks from Excel
          </h1>
          <p className="text-gray-600">
            Upload an Excel file to bulk import track data
          </p>
        </div>

        {/* Instructions */}
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-6 mb-6">
          <h3 className="text-lg font-semibold text-blue-900 mb-3 flex items-center gap-2">
            <span>📋</span>
            <span>Excel File Format</span>
          </h3>
          <div className="text-sm text-blue-800 space-y-2">
            <p>
              Your Excel file should have the following columns (in any order):
            </p>
            <ul className="list-disc list-inside ml-4 space-y-1">
              <li><strong>TrackingNumber</strong> (required) - Unique tracking ID</li>
              <li><strong>ClientCode</strong> (required) - Client identifier</li>
              <li><strong>Status</strong> - Created, InTransit, Delivered, etc.</li>
              <li><strong>Description</strong> - Brief description</li>
              <li><strong>Weight</strong> - Weight in kg (number)</li>
              <li><strong>DeclaredValue</strong> - Value in dollars (number)</li>
              <li><strong>OriginCountry</strong> - Country of origin</li>
              <li><strong>DestinationCountry</strong> - Destination country</li>
              <li><strong>ShippedAt</strong> - Date shipped (YYYY-MM-DD)</li>
              <li><strong>EstimatedDeliveryAt</strong> - Expected delivery (YYYY-MM-DD)</li>
            </ul>
            <p className="mt-3 font-medium">
              💡 Tip: First row should contain column headers
            </p>
          </div>
        </div>

        {/* Upload Area */}
        <div className="bg-white border border-gray-200 rounded-xl p-6 mb-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Upload File
          </h3>

          {/* Drag & Drop Area */}
          <div
            onDragOver={handleDragOver}
            onDrop={handleDrop}
            className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center hover:border-blue-400 transition-colors"
          >
            <input
              ref={fileInputRef}
              type="file"
              accept=".xlsx,.xls"
              onChange={handleFileSelect}
              className="hidden"
              id="file-upload"
            />

            {selectedFile ? (
              <div className="space-y-4">
                <div className="flex items-center justify-center gap-3">
                  <span className="text-4xl">📄</span>
                  <div className="text-left">
                    <p className="font-medium text-gray-900">{selectedFile.name}</p>
                    <p className="text-sm text-gray-500">
                      {(selectedFile.size / 1024).toFixed(2)} KB
                    </p>
                  </div>
                </div>
                <div className="flex items-center justify-center gap-3">
                  <button
                    onClick={handleUpload}
                    disabled={isUploading}
                    className="px-6 py-3 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors disabled:bg-blue-400 disabled:cursor-not-allowed flex items-center gap-2"
                  >
                    {isUploading ? (
                      <>
                        <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                        <span>Uploading...</span>
                      </>
                    ) : (
                      <>
                        <span>⬆️</span>
                        <span>Upload & Import</span>
                      </>
                    )}
                  </button>
                  <button
                    onClick={handleClearFile}
                    disabled={isUploading}
                    className="px-6 py-3 bg-gray-100 text-gray-700 rounded-lg font-medium hover:bg-gray-200 transition-colors disabled:opacity-50"
                  >
                    Clear
                  </button>
                </div>
              </div>
            ) : (
              <div>
                <span className="text-6xl mb-4 block">📁</span>
                <p className="text-gray-600 mb-4">
                  Drag and drop your Excel file here, or
                </p>
                <label
                  htmlFor="file-upload"
                  className="inline-block px-6 py-3 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors cursor-pointer"
                >
                  Choose File
                </label>
                <p className="text-sm text-gray-500 mt-3">
                  Supported formats: .xlsx, .xls
                </p>
              </div>
            )}
          </div>
        </div>

        {/* Result */}
        {result && (
          <div className="bg-white border border-gray-200 rounded-xl p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <span>✅</span>
              <span>Import Results</span>
            </h3>

            <div className="grid grid-cols-2 gap-4 mb-6">
              <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                <p className="text-sm text-green-700 mb-1">Successful</p>
                <p className="text-3xl font-bold text-green-900">
                  {result.successCount}
                </p>
              </div>
              <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <p className="text-sm text-red-700 mb-1">Failed</p>
                <p className="text-3xl font-bold text-red-900">
                  {result.failedCount}
                </p>
              </div>
            </div>

            {result.errors.length > 0 && (
              <div>
                <h4 className="font-semibold text-gray-900 mb-2">Errors:</h4>
                <div className="bg-red-50 border border-red-200 rounded-lg p-4 max-h-60 overflow-y-auto">
                  <ul className="space-y-1 text-sm text-red-700">
                    {result.errors.map((error, index) => (
                      <li key={index} className="flex items-start gap-2">
                        <span>•</span>
                        <span>{error}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            )}

            {result.successCount > 0 && result.failedCount === 0 && (
              <div className="bg-green-50 border border-green-200 rounded-lg p-4 text-center">
                <p className="text-green-800 font-medium">
                  🎉 All tracks imported successfully!
                </p>
              </div>
            )}
          </div>
        )}

        {/* Sample Template Info */}
        <div className="bg-gray-50 border border-gray-200 rounded-xl p-6 mt-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-3">
            Need a Template?
          </h3>
          <p className="text-gray-600 mb-4">
            Create an Excel file with the column headers listed above, or use this example structure:
          </p>
          <div className="bg-white border border-gray-300 rounded-lg p-4 overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead>
                <tr className="border-b">
                  <th className="px-3 py-2 text-left">TrackingNumber</th>
                  <th className="px-3 py-2 text-left">ClientCode</th>
                  <th className="px-3 py-2 text-left">Status</th>
                  <th className="px-3 py-2 text-left">Description</th>
                  <th className="px-3 py-2 text-left">Weight</th>
                </tr>
              </thead>
              <tbody>
                <tr className="border-b">
                  <td className="px-3 py-2">TR-001</td>
                  <td className="px-3 py-2">CLT-12345678</td>
                  <td className="px-3 py-2">InTransit</td>
                  <td className="px-3 py-2">Electronics</td>
                  <td className="px-3 py-2">2.5</td>
                </tr>
                <tr>
                  <td className="px-3 py-2">TR-002</td>
                  <td className="px-3 py-2">CLT-87654321</td>
                  <td className="px-3 py-2">Created</td>
                  <td className="px-3 py-2">Documents</td>
                  <td className="px-3 py-2">0.5</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </ManagerLayout>
  );
};

export default ImportExcel;
