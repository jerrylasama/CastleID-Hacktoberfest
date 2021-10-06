mov al, 'X'
call fungsi_print
mov al, 'C'

fungsi_print:
	mov ah, 0x0e
	int 0x10
	call fungsi_after
	int 0x10
	ret

fungsi_after:
	ret

jmp $

times 510-($-$$) db 0

dw 0xaa55
