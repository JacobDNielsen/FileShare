import { useState, useEffect, useCallback } from 'react';
import { Button, Alert, Spinner } from 'react-bootstrap';
import apiClient from '../api/apiClient';
import type { FileListItem, PagedResult } from '../models/types';
import FileTable from '../components/FileTable';
import FileUploadModal from '../components/FileUploadModal';
import ConfirmDialog from '../components/ConfirmDialog';
import Pagination from '../components/Pagination';

const PAGE_SIZE = 20;

export default function FilesPage() {
  const [files, setFiles] = useState<FileListItem[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showUpload, setShowUpload] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<{
    fileId: string;
    fileName: string;
  } | null>(null);

  const fetchFiles = useCallback(async (p: number) => {
    setLoading(true);
    setError('');
    try {
      const { data } = await apiClient.get<PagedResult<FileListItem>>('/storage/paged', {
        params: { Page: p, PageSize: PAGE_SIZE },
      });
      setFiles(data.items);
      setPage(data.page);
      setTotalPages(data.totalPages);
    } catch {
      setError('Failed to load files.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchFiles(1);
  }, [fetchFiles]);

  const handleDownload = async (fileId: string, fileName: string) => {
    try {
      const { data } = await apiClient.get(`/storage/${encodeURIComponent(fileId)}/download`, {
        responseType: 'blob',
      });
      const url = URL.createObjectURL(data);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      a.click();
      URL.revokeObjectURL(url);
    } catch {
      setError('Download failed.');
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await apiClient.delete(`/storage/${encodeURIComponent(deleteTarget.fileId)}`);
      setDeleteTarget(null);
      fetchFiles(page);
    } catch {
      setError('Delete failed.');
      setDeleteTarget(null);
    }
  };

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2>Files</h2>
        <Button variant="primary" onClick={() => setShowUpload(true)}>
          Upload File
        </Button>
      </div>

      {error && <Alert variant="danger" dismissible onClose={() => setError('')}>{error}</Alert>}

      {loading ? (
        <div className="text-center mt-4">
          <Spinner animation="border" />
        </div>
      ) : (
        <>
          <FileTable
            files={files}
            onDelete={(fileId, fileName) => setDeleteTarget({ fileId, fileName })}
            onDownload={handleDownload}
          />
          <Pagination page={page} totalPages={totalPages} onPageChange={fetchFiles} />
        </>
      )}

      <FileUploadModal
        show={showUpload}
        onClose={() => setShowUpload(false)}
        onUploaded={() => fetchFiles(page)}
      />

      <ConfirmDialog
        show={!!deleteTarget}
        title="Delete File"
        message={`Are you sure you want to delete "${deleteTarget?.fileName}"?`}
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
