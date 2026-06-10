import { Table, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import type { FileListItem } from '../models/types';

interface Props {
  files: FileListItem[];
  onDelete: (fileId: string, fileName: string) => void;
  onDownload: (fileId: string, fileName: string) => void;
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export default function FileTable({ files, onDelete, onDownload }: Props) {
  const navigate = useNavigate();

  if (files.length === 0) {
    return <p className="text-muted mt-3">No files found.</p>;
  }

  return (
    <Table striped bordered hover responsive className="mt-3">
      <thead>
        <tr>
          <th>Name</th>
          <th>Size</th>
          <th>Last Modified</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {files.map((f) => (
          <tr key={f.fileId}>
            <td>{f.baseFileName}</td>
            <td>{formatSize(f.size)}</td>
            <td>{new Date(f.lastModifiedAt).toLocaleString()}</td>
            <td>
              <Button
                variant="outline-primary"
                size="sm"
                className="me-1"
                onClick={() => navigate(`/files/${f.fileId}`)}
              >
                Details
              </Button>
              <Button
                variant="outline-success"
                size="sm"
                className="me-1"
                onClick={() => onDownload(f.fileId, f.baseFileName)}
              >
                Download
              </Button>
              <Button
                variant="outline-danger"
                size="sm"
                onClick={() => onDelete(f.fileId, f.baseFileName)}
              >
                Delete
              </Button>
            </td>
          </tr>
        ))}
      </tbody>
    </Table>
  );
}
